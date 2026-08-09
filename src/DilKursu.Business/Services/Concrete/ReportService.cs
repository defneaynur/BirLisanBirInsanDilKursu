using DilKursu.Business.Common;
using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace DilKursu.Business.Services.Concrete;

public class ReportService(IUnitOfWork uow) : IReportService
{
    /// <summary>
    /// Tüm aktif derslerin doluluk durumunu (kayıtlı öğrenci / kontenjan) döndürür.
    /// </summary>
    /// <returns></returns>
    public async Task<ServiceResult<IReadOnlyList<CourseOccupancyDto>>> GetCourseOccupancyAsync()
    {
        // Dersler; ad ve doluluk için dil/şube/kayıt ilişkileriyle çekilir.
        var courses = await uow.Courses.Query()
            .Include(c => c.Language)
            .Include(c => c.Branch)
            .Include(c => c.Enrollments)
            .AsNoTracking()
            .ToListAsync();

        var list = courses.Select(c =>
        {
            var enrolled = c.Enrollments.Count(e => e.IsActive);
            return new CourseOccupancyDto
            {
                CourseName = $"{c.Language.Name} {c.Level} / {c.Branch.Name}",
                BranchName = c.Branch.Name,
                Enrolled = enrolled,
                Quota = c.Quota,
                // Kontenjan 0 ise bölme hatasını önlemek için yüzde 0 kabul edilir.
                OccupancyPercent = c.Quota > 0 ? (int)Math.Round(enrolled * 100.0 / c.Quota) : 0
            };
        }).ToList();

        return ServiceResult<IReadOnlyList<CourseOccupancyDto>>.Ok(list);
    }

    /// <summary>
    /// Şube başına açılmış ders sayısını gruplayarak döndürür (dağılım grafiği için).
    /// </summary>
    /// <returns></returns>
    public async Task<ServiceResult<IReadOnlyList<NameCountDto>>> GetBranchCourseDistributionAsync()
    {
        // Dersler şubeye göre gruplanarak sayılır.
        var courses = await uow.Courses.Query()
            .Include(c => c.Branch)
            .AsNoTracking()
            .ToListAsync();

        var list = courses
            .GroupBy(c => c.Branch.Name)
            .Select(g => new NameCountDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        return ServiceResult<IReadOnlyList<NameCountDto>>.Ok(list);
    }

    /// <summary>
    /// Dil başına açılmış ders sayısını gruplayarak döndürür (dağılım grafiği için).
    /// </summary>
    /// <returns></returns>
    public async Task<ServiceResult<IReadOnlyList<NameCountDto>>> GetLanguageCourseDistributionAsync()
    {
        // Dersler dile göre gruplanarak sayılır.
        var courses = await uow.Courses.Query()
            .Include(c => c.Language)
            .AsNoTracking()
            .ToListAsync();

        var list = courses
            .GroupBy(c => c.Language.Name)
            .Select(g => new NameCountDto { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        return ServiceResult<IReadOnlyList<NameCountDto>>.Ok(list);
    }
}
