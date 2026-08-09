using DilKursu.DataAccess.Repositories;
using DilKursu.Entities;

namespace DilKursu.DataAccess.UnitOfWork;

/// <summary>
/// Birden fazla depo (repository) üzerinde yapılan değişiklikleri tek bir işlem (transaction) olarak
/// yöneten Unit of Work soyutlaması. Tüm depolar aynı <see cref="Context.AppDbContext"/> örneğini
/// paylaştığından, <see cref="SaveChangesAsync"/> çağrısı tüm değişiklikleri atomik olarak kaydeder.
/// İş katmanı yalnızca bu arayüze bağımlıdır (Dependency Inversion Principle).
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    /// <summary>Şube deposu.</summary>
    IGenericRepository<Branch> Branches { get; }

    /// <summary>Derslik deposu.</summary>
    IGenericRepository<Classroom> Classrooms { get; }

    /// <summary>Dil deposu.</summary>
    IGenericRepository<Language> Languages { get; }

    /// <summary>Öğretmen deposu.</summary>
    IGenericRepository<Teacher> Teachers { get; }

    /// <summary>Öğretmen-Dil bağlantı deposu.</summary>
    IGenericRepository<TeacherLanguage> TeacherLanguages { get; }

    /// <summary>Öğretmen-Şube bağlantı deposu.</summary>
    IGenericRepository<TeacherBranch> TeacherBranches { get; }

    /// <summary>Öğretmen müsaitlik deposu.</summary>
    IGenericRepository<TeacherAvailability> TeacherAvailabilities { get; }

    /// <summary>Ders (kurs) deposu.</summary>
    IGenericRepository<Course> Courses { get; }

    /// <summary>Öğrenci deposu.</summary>
    IGenericRepository<Student> Students { get; }

    /// <summary>Kayıt deposu.</summary>
    IGenericRepository<Enrollment> Enrollments { get; }

    /// <summary>Taksit deposu.</summary>
    IGenericRepository<Installment> Installments { get; }

    /// <summary>Denetim (audit) kaydı deposu.</summary>
    IGenericRepository<AuditLog> AuditLogs { get; }

    /// <summary>Hata (exception) kaydı deposu.</summary>
    IGenericRepository<ErrorLog> ErrorLogs { get; }

    /// <summary>
    /// Bağlamda biriken tüm değişiklikleri tek seferde veritabanına yazar.
    /// </summary>
    /// <param name="cancellationToken">İşlemi iptal etmek için token.</param>
    /// <returns>Etkilenen satır sayısı.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
