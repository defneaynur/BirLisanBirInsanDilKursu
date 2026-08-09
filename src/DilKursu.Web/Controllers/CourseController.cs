using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DilKursu.Web.Infrastructure.Auditing;

namespace DilKursu.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class CourseController(ICourseService courseService, ILanguageService languageService, IBranchService branchService) : Controller
{
    /// <summary>Ders listeleme sayfasını döndürür.</summary>
    [HttpGet]
    public IActionResult Index() => View();

    /// <summary>Ders açma sihirbazı sayfasını döndürür.</summary>
    [HttpGet]
    public IActionResult Create() => View();

    /// <summary>Tüm dersleri JSON olarak döndürür (DataTables kaynağı).</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await courseService.GetAllAsync();
        return Json(result.Data);
    }

    /// <summary>Dil seçeneklerini JSON döndürür (sihirbaz için).</summary>
    [HttpGet]
    public async Task<IActionResult> LanguageOptions()
    {
        var result = await languageService.GetAllAsync();
        return Json(result.Data?.Select(l => new { id = l.Id, name = l.Name }));
    }

    /// <summary>Şube seçeneklerini JSON döndürür (sihirbaz için).</summary>
    [HttpGet]
    public async Task<IActionResult> BranchOptions()
    {
        var result = await branchService.GetAllAsync();
        return Json(result.Data?.Select(b => new { id = b.Id, name = b.Name }));
    }

    /// <summary>
    /// Verilen kriterlere göre uygun öğretmen ve boş derslik önerilerini AJAX ile döndürür.
    /// Sihirbazın "Uygunları Getir" adımını besler.
    /// </summary>
    /// <param name="criteria">Ders açma kriterleri (dil, şube, gün, saat).</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suggest([FromBody] CourseCriteriaDto criteria)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Lütfen tüm kriterleri doldurun." });
        }

        var result = await courseService.GetSuggestionsAsync(criteria);
        return result.Success
            ? Json(new { success = true, data = result.Data })
            : Json(new { success = false, message = result.Message });
    }

    /// <summary>
    /// Sihirbazda seçilen öğretmen ve derslik ile yeni bir ders oluşturur .
    /// </summary>
    /// <param name="dto">Ders oluşturma verisi.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Ders", "Ekleme")]
    public async Task<IActionResult> Create([FromBody] CourseCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Lütfen tüm alanları doldurun." });
        }

        var result = await courseService.CreateAsync(dto);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>Bir dersi siler .</summary>
    /// <param name="id">Silinecek ders kimliği.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Ders", "Silme")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await courseService.DeleteAsync(id);
        return Json(new { success = result.Success, message = result.Message });
    }
}
