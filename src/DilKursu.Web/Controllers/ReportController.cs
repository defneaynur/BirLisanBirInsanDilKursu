using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DilKursu.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class ReportController(IReportService reportService) : Controller
{
    /// <summary>Rapor/grafik sayfasını döndürür.</summary>
    [HttpGet]
    public IActionResult Index() => View();

    /// <summary>Ders doluluk verisini JSON döndürür (doluluk çubuk grafiği için).</summary>
    [HttpGet]
    public async Task<IActionResult> CourseOccupancy()
    {
        var result = await reportService.GetCourseOccupancyAsync();
        return Json(result.Data);
    }

    /// <summary>Şube başına ders dağılımını JSON döndürür (dağılım grafiği için).</summary>
    [HttpGet]
    public async Task<IActionResult> BranchDistribution()
    {
        var result = await reportService.GetBranchCourseDistributionAsync();
        return Json(result.Data);
    }

    /// <summary>Dil başına ders dağılımını JSON döndürür (dağılım grafiği için).</summary>
    [HttpGet]
    public async Task<IActionResult> LanguageDistribution()
    {
        var result = await reportService.GetLanguageCourseDistributionAsync();
        return Json(result.Data);
    }

    /// <summary>Aylık tahsilat (gelir) trendini JSON döndürür (çizgi grafiği için).</summary>
    [HttpGet]
    public async Task<IActionResult> MonthlyCollection()
    {
        var result = await reportService.GetMonthlyCollectionAsync();
        return Json(result.Data);
    }

    /// <summary>Aylık yeni kayıt trendini JSON döndürür (çizgi grafiği için).</summary>
    [HttpGet]
    public async Task<IActionResult> MonthlyEnrollments()
    {
        var result = await reportService.GetMonthlyEnrollmentsAsync();
        return Json(result.Data);
    }

    /// <summary>Haftalık ders yoğunluğunu JSON döndürür (çubuk grafiği için).</summary>
    [HttpGet]
    public async Task<IActionResult> WeeklyDensity()
    {
        var result = await reportService.GetWeeklyCourseDensityAsync();
        return Json(result.Data);
    }
}
