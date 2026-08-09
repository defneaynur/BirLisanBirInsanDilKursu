using DilKursu.DataAccess.Identity;
using DilKursu.Entities;
using DilKursu.Entities.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace DilKursu.DataAccess.Context;

/// <param name="options">Veritabanı sağlayıcısı ve bağlantı ayarlarını içeren seçenekler (DI ile sağlanır).</param>
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<Classroom> Classrooms => Set<Classroom>();

    public DbSet<Language> Languages => Set<Language>();

    public DbSet<Teacher> Teachers => Set<Teacher>();

    public DbSet<TeacherLanguage> TeacherLanguages => Set<TeacherLanguage>();

    public DbSet<TeacherBranch> TeacherBranches => Set<TeacherBranch>();

    public DbSet<TeacherAvailability> TeacherAvailabilities => Set<TeacherAvailability>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Student> Students => Set<Student>();

    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    public DbSet<Installment> Installments => Set<Installment>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();

    /// <summary>
    /// Model oluşturulurken çalışır. Aynı assembly içindeki tüm
    /// <see cref="IEntityTypeConfiguration{TEntity}"/> yapılandırmalarını otomatik uygular.
    /// Böylece her varlığın Fluent API kuralları kendi dosyasında izole edilir (SRP).
    /// </summary>
    /// <param name="modelBuilder">EF Core model oluşturucu.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity tablolarının varsayılan yapılandırması için temel sınıf çağrılır.
        base.OnModelCreating(modelBuilder);

        // Identity tablolarının varsayılan "AspNet..." adları yerine, projedeki diğer tablolarla
        // tutarlı sade adlar kullanılır.
        modelBuilder.Entity<ApplicationUser>().ToTable("Users");
        modelBuilder.Entity<IdentityRole>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");

        // Bu assembly'deki tüm IEntityTypeConfiguration sınıflarını topluca uygular.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    /// <summary>
    /// Değişiklikleri kaydetmeden önce denetim (audit) alanlarını otomatik doldurur:
    /// yeni kayıtlarda CreatedDate, güncellenen kayıtlarda UpdatedDate ayarlanır.
    /// </summary>
    /// <param name="cancellationToken">İşlemi iptal etmek için token.</param>
    /// <returns>Etkilenen satır sayısı.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Takip edilen ve BaseEntity'den türeyen tüm girdileri denetim alanları için işaretle.
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = DateTime.Now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedDate = DateTime.Now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
