using System.Linq.Expressions;
using DilKursu.DataAccess.Context;
using DilKursu.Entities.Common;
using Microsoft.EntityFrameworkCore;

namespace DilKursu.DataAccess.Repositories;

public class GenericRepository<TEntity>(AppDbContext context)
    : IGenericRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>Paylaşılan veritabanı bağlamı.</summary>
    protected readonly AppDbContext Context = context;

    /// <summary>İlgili varlığın DbSet nesnesi.</summary>
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    /// <summary>
    /// Verilen id'ye sahip aktif varlığı döndürür; bulunamazsa null döner.
    /// </summary>
    /// <param name="id">Varlığın birincil anahtarı.</param>
    /// <returns>Bulunan varlık veya null.</returns>
    public async Task<TEntity?> GetByIdAsync(int id)
    {
        // Yalnızca aktif kayıtlar arasından birincil anahtara göre arar.
        return await DbSet.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
    }

    /// <summary>
    /// Filtreye ve include ifadelerine göre tek bir aktif varlık döndürür.
    /// </summary>
    /// <param name="predicate">Filtre koşulu.</param>
    /// <param name="includes">Birlikte yüklenecek ilişkili navigasyon özellikleri.</param>
    /// <returns>Koşula uyan varlık veya null.</returns>
    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate,
                                         params Expression<Func<TEntity, object>>[] includes)
    {
        // Aktif kayıt filtresiyle başlayan sorguya istenen include'lar eklenir.
        IQueryable<TEntity> query = DbSet.Where(e => e.IsActive);
        query = ApplyIncludes(query, includes);
        return await query.FirstOrDefaultAsync(predicate);
    }

    /// <summary>
    /// Tüm aktif varlıkları, isteğe bağlı include ifadeleriyle birlikte döndürür.
    /// </summary>
    /// <param name="includes">Birlikte yüklenecek ilişkili navigasyon özellikleri.</param>
    /// <returns>Aktif varlıkların listesi.</returns>
    public async Task<IReadOnlyList<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = DbSet.Where(e => e.IsActive);
        query = ApplyIncludes(query, includes);
        return await query.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Belirtilen koşulu sağlayan aktif varlıkları, isteğe bağlı include ifadeleriyle döndürür.
    /// </summary>
    /// <param name="predicate">Filtre koşulu.</param>
    /// <param name="includes">Birlikte yüklenecek ilişkili navigasyon özellikleri.</param>
    /// <returns>Koşula uyan varlıkların listesi.</returns>
    public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate,
                                                        params Expression<Func<TEntity, object>>[] includes)
    {
        IQueryable<TEntity> query = DbSet.Where(e => e.IsActive).Where(predicate);
        query = ApplyIncludes(query, includes);
        return await query.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Belirtilen koşulu sağlayan aktif bir kayıt olup olmadığını döndürür.
    /// </summary>
    /// <param name="predicate">Kontrol edilecek koşul.</param>
    /// <returns>Koşula uyan kayıt varsa true.</returns>
    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
    {
        // Aktif kayıtlar arasında koşula uyan en az bir kayıt var mı?
        return await DbSet.Where(e => e.IsActive).AnyAsync(predicate);
    }

    /// <summary>
    /// Yeni bir varlığı bağlama ekler (kaydetme UnitOfWork ile yapılır).
    /// </summary>
    /// <param name="entity">Eklenecek varlık.</param>
    public async Task AddAsync(TEntity entity)
    {
        // Varlık bağlama eklenir; fiziksel yazma SaveChanges (UnitOfWork) ile gerçekleşir.
        await DbSet.AddAsync(entity);
    }

    /// <summary>
    /// Mevcut bir varlığı güncellenmiş olarak işaretler.
    /// </summary>
    /// <param name="entity">Güncellenecek varlık.</param>
    public void Update(TEntity entity)
    {
        // Varlık değiştirilmiş olarak işaretlenir.
        DbSet.Update(entity);
    }

    /// <summary>
    /// Bir varlığı yumuşak silme (soft delete) ile pasifleştirir (IsActive = false).
    /// </summary>
    /// <param name="entity">Pasifleştirilecek varlık.</param>
    public void Remove(TEntity entity)
    {
        // Fiziksel silme yerine yumuşak silme uygulanır: kayıt pasifleştirilir.
        entity.IsActive = false;
        DbSet.Update(entity);
    }

    /// <summary>
    /// İleri düzey sorgular için, yalnızca aktif kayıtları içeren temel sorgu nesnesini döndürür.
    /// </summary>
    /// <returns>Aktif kayıtları içeren sorgu.</returns>
    public IQueryable<TEntity> Query()
    {
        // İş katmanının kompozisyon kurabilmesi için aktif kayıtları içeren temel sorgu.
        return DbSet.Where(e => e.IsActive);
    }

    /// <summary>
    /// Verilen include ifadelerini sorguya uygular. Ortak yardımcı metot (DRY).
    /// </summary>
    /// <param name="query">Genişletilecek sorgu.</param>
    /// <param name="includes">Uygulanacak include ifadeleri.</param>
    /// <returns>Include'ları uygulanmış sorgu.</returns>
    private static IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query,
                                                     Expression<Func<TEntity, object>>[] includes)
    {
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return query;
    }
}
