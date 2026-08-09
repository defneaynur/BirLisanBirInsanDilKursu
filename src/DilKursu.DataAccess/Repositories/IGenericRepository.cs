using System.Linq.Expressions;
using DilKursu.Entities.Common;

namespace DilKursu.DataAccess.Repositories;

public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Verilen kimliğe sahip aktif varlığı getirir; bulunamazsa null döner.
    /// </summary>
    /// <param name="id">Varlığın birincil anahtarı.</param>
    Task<TEntity?> GetByIdAsync(int id);

    /// <summary>
    /// İsteğe bağlı bir filtreye ve include (ilişkili veri yükleme) ifadelerine göre tek bir varlık getirir.
    /// </summary>
    /// <param name="predicate">Filtre koşulu.</param>
    /// <param name="includes">Birlikte yüklenecek ilişkili navigasyon özellikleri.</param>
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate,
                            params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Tüm aktif varlıkları, isteğe bağlı include ifadeleriyle birlikte listeler.
    /// </summary>
    /// <param name="includes">Birlikte yüklenecek ilişkili navigasyon özellikleri.</param>
    Task<IReadOnlyList<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Belirtilen koşulu sağlayan aktif varlıkları, isteğe bağlı include ifadeleriyle listeler.
    /// </summary>
    /// <param name="predicate">Filtre koşulu.</param>
    /// <param name="includes">Birlikte yüklenecek ilişkili navigasyon özellikleri.</param>
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate,
                                           params Expression<Func<TEntity, object>>[] includes);

    /// <summary>
    /// Belirtilen koşulu sağlayan aktif bir kayıt olup olmadığını kontrol eder.
    /// Çakışma/benzersizlik kontrolleri için kullanışlıdır.
    /// </summary>
    /// <param name="predicate">Kontrol edilecek koşul.</param>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Yeni bir varlığı bağlama ekler (henüz veritabanına yazılmaz; kaydetme UnitOfWork ile yapılır).
    /// </summary>
    /// <param name="entity">Eklenecek varlık.</param>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// Mevcut bir varlığı güncellenmiş olarak işaretler.
    /// </summary>
    /// <param name="entity">Güncellenecek varlık.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Bir varlığı yumuşak silme (soft delete) ile pasifleştirir (IsActive = false).
    /// </summary>
    /// <param name="entity">Pasifleştirilecek varlık.</param>
    void Remove(TEntity entity);

    /// <summary>
    /// İleri düzey sorgular oluşturabilmek için filtrelenmemiş (henüz çalıştırılmamış) sorgu nesnesi döndürür.
    /// Yalnızca aktif kayıtları içerir. Karmaşık iş sorgularında (ör. çakışma tespiti) kullanılır.
    /// </summary>
    IQueryable<TEntity> Query();
}
