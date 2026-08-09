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
}
