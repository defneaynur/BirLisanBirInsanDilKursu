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

    // Türkçe ay kısaltmaları (kültürden bağımsız, tutarlı etiketler için).
    private static readonly string[] TrMonths =
        { "Oca", "Şub", "Mar", "Nis", "May", "Haz", "Tem", "Ağu", "Eyl", "Eki", "Kas", "Ara" };

    /// <summary>
    /// Son 6 ayda tahsil edilen (ödenmiş) taksit tutarlarını aya göre toplar.
    /// Boş aylar da 0 olarak eklenir ki trend çizgisi kesintisiz görünsün.
    /// </summary>
    /// <returns>Aylık tahsilat tutarı listesi (eskiden yeniye).</returns>
    public async Task<ServiceResult<IReadOnlyList<NameAmountDto>>> GetMonthlyCollectionAsync()
    {
        // Son 6 ayın başlangıcı (bulunulan ayın 1'i - 5 ay).
        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);

        // Ödenmiş taksitler; ödeme tarihi pencereye giren kayıtlar bellek tarafında gruplanır.
        var paid = await uow.Installments.Query()
            .Where(i => i.IsPaid && i.PaidDate != null && i.PaidDate >= start)
            .AsNoTracking()
            .Select(i => new { Date = i.PaidDate!.Value, i.Amount })
            .ToListAsync();

        var list = new List<NameAmountDto>();
        for (var k = 0; k < 6; k++)
        {
            var m = start.AddMonths(k);
            var total = paid
                .Where(p => p.Date.Year == m.Year && p.Date.Month == m.Month)
                .Sum(p => p.Amount);
            list.Add(new NameAmountDto { Name = $"{TrMonths[m.Month - 1]} {m.Year}", Amount = total });
        }

        return ServiceResult<IReadOnlyList<NameAmountDto>>.Ok(list);
    }

    /// <summary>
    /// Son 6 ayda yapılan yeni kayıt (Enrollment) sayılarını aya göre toplar.
    /// Boş aylar da 0 olarak eklenir ki trend çizgisi kesintisiz görünsün.
    /// </summary>
    /// <returns>Aylık kayıt sayısı listesi (eskiden yeniye).</returns>
    public async Task<ServiceResult<IReadOnlyList<NameCountDto>>> GetMonthlyEnrollmentsAsync()
    {
        var start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);

        var dates = await uow.Enrollments.Query()
            .Where(e => e.EnrollmentDate >= start)
            .AsNoTracking()
            .Select(e => e.EnrollmentDate)
            .ToListAsync();

        var list = new List<NameCountDto>();
        for (var k = 0; k < 6; k++)
        {
            var m = start.AddMonths(k);
            var count = dates.Count(d => d.Year == m.Year && d.Month == m.Month);
            list.Add(new NameCountDto { Name = $"{TrMonths[m.Month - 1]} {m.Year}", Count = count });
        }

        return ServiceResult<IReadOnlyList<NameCountDto>>.Ok(list);
    }

    /// <summary>
    /// Açılmış dersleri haftanın günlerine göre sayar (Pazartesi–Pazar sırasıyla).
    /// Ders olmayan günler de 0 olarak eklenir; boş/yoğun günler bir bakışta görülür.
    /// </summary>
    /// <returns>Gün bazlı ders sayısı listesi.</returns>
    public async Task<ServiceResult<IReadOnlyList<NameCountDto>>> GetWeeklyCourseDensityAsync()
    {
        var days = await uow.Courses.Query()
            .AsNoTracking()
            .Select(c => c.Day)
            .ToListAsync();

        // Hafta içi–hafta sonu okunabilirliği için Pazartesi'den Pazar'a sabit sıra.
        var order = new[]
        {
            (DayOfWeek.Monday, "Pazartesi"), (DayOfWeek.Tuesday, "Salı"),
            (DayOfWeek.Wednesday, "Çarşamba"), (DayOfWeek.Thursday, "Perşembe"),
            (DayOfWeek.Friday, "Cuma"), (DayOfWeek.Saturday, "Cumartesi"),
            (DayOfWeek.Sunday, "Pazar")
        };

        var list = order
            .Select(o => new NameCountDto { Name = o.Item2, Count = days.Count(d => d == o.Item1) })
            .ToList();

        return ServiceResult<IReadOnlyList<NameCountDto>>.Ok(list);
    }
}
