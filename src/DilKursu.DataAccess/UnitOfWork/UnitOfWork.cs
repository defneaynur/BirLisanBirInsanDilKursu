using DilKursu.DataAccess.Context;
using DilKursu.DataAccess.Repositories;
using DilKursu.Entities;

namespace DilKursu.DataAccess.UnitOfWork;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    // Depoların oluşturulması için arka alanlar.
    private IGenericRepository<Branch>? _branches;
    private IGenericRepository<Classroom>? _classrooms;
    private IGenericRepository<Language>? _languages;
    private IGenericRepository<Teacher>? _teachers;
    private IGenericRepository<TeacherLanguage>? _teacherLanguages;
    private IGenericRepository<TeacherBranch>? _teacherBranches;
    private IGenericRepository<TeacherAvailability>? _teacherAvailabilities;
    private IGenericRepository<Course>? _courses;
    private IGenericRepository<Student>? _students;
    private IGenericRepository<Enrollment>? _enrollments;
    private IGenericRepository<Installment>? _installments;
    private IGenericRepository<AuditLog>? _auditLogs;
    private IGenericRepository<ErrorLog>? _errorLogs;

    /// <summary>Şube deposu.</summary>
    public IGenericRepository<Branch> Branches => _branches ??= new GenericRepository<Branch>(context);

    /// <summary>Derslik deposu.</summary>
    public IGenericRepository<Classroom> Classrooms => _classrooms ??= new GenericRepository<Classroom>(context);

    /// <summary>Dil deposu.</summary>
    public IGenericRepository<Language> Languages => _languages ??= new GenericRepository<Language>(context);

    /// <summary>Öğretmen deposu.</summary>
    public IGenericRepository<Teacher> Teachers => _teachers ??= new GenericRepository<Teacher>(context);

    /// <summary>Öğretmen-Dil bağlantı deposu.</summary>
    public IGenericRepository<TeacherLanguage> TeacherLanguages =>
        _teacherLanguages ??= new GenericRepository<TeacherLanguage>(context);

    /// <summary>Öğretmen-Şube bağlantı deposu.</summary>
    public IGenericRepository<TeacherBranch> TeacherBranches =>
        _teacherBranches ??= new GenericRepository<TeacherBranch>(context);

    /// <summary>Öğretmen müsaitlik deposu.</summary>
    public IGenericRepository<TeacherAvailability> TeacherAvailabilities =>
        _teacherAvailabilities ??= new GenericRepository<TeacherAvailability>(context);

    /// <summary>Ders (kurs) deposu.</summary>
    public IGenericRepository<Course> Courses => _courses ??= new GenericRepository<Course>(context);

    /// <summary>Öğrenci deposu.</summary>
    public IGenericRepository<Student> Students => _students ??= new GenericRepository<Student>(context);

    /// <summary>Kayıt deposu.</summary>
    public IGenericRepository<Enrollment> Enrollments => _enrollments ??= new GenericRepository<Enrollment>(context);

    /// <summary>Taksit deposu.</summary>
    public IGenericRepository<Installment> Installments => _installments ??= new GenericRepository<Installment>(context);

    /// <summary>Denetim (audit) kaydı deposu.</summary>
    public IGenericRepository<AuditLog> AuditLogs => _auditLogs ??= new GenericRepository<AuditLog>(context);

    /// <summary>Hata (exception) kaydı deposu.</summary>
    public IGenericRepository<ErrorLog> ErrorLogs => _errorLogs ??= new GenericRepository<ErrorLog>(context);

    /// <summary>
    /// Bağlamda biriken tüm değişiklikleri tek işlemde (atomik) veritabanına kaydeder.
    /// </summary>
    /// <param name="cancellationToken">İşlemi iptal etmek için token.</param>
    /// <returns>Etkilenen satır sayısı.</returns>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Tüm depoların paylaştığı bağlamdaki değişiklikleri tek işlemde kaydeder.
        return context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Bağlamı asenkron olarak serbest bırakır (bağlantıların düzgün kapanması için).
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
