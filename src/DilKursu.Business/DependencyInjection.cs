using DilKursu.Business.Services.Abstract;
using DilKursu.Business.Services.Concrete;
using DilKursu.DataAccess.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace DilKursu.Business;

public static class DependencyInjection
{
    /// <summary>
    /// Unit of Work ve tüm uygulama servislerini kapsam (scoped) ömrüyle kaydeder.
    /// Scoped ömür, her HTTP isteğinde tek bir DbContext/UnitOfWork örneği kullanılmasını sağlar.
    /// </summary>
    /// <param name="services">Servis koleksiyonu.</param>
    /// <returns>Zincirleme kullanım için servis koleksiyonu.</returns>
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Veri erişim işlem yöneticisi (tüm servisler bunun üzerinden çalışır).
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IClassroomService, ClassroomService>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<ITeacherService, TeacherService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IErrorLogService, ErrorLogService>();

        return services;
    }
}
