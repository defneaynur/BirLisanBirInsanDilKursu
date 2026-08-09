using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DilKursu.Web.Infrastructure.Auditing;

namespace DilKursu.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class LanguageController(ILanguageService languageService) : Controller
{
    /// <summary>Dil listeleme sayfasını döndürür.</summary>
    [HttpGet]
    public IActionResult Index() => View();

    /// <summary>Tüm dilleri JSON olarak döndürür (DataTables kaynağı).</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await languageService.GetAllAsync();
        return Json(result.Data);
    }

    /// <summary>Tek bir dili düzenleme için JSON döndürür.</summary>
    /// <param name="id">Dil kimliği.</param>
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var result = await languageService.GetByIdAsync(id);
        return result.Success
            ? Json(new { success = true, data = result.Data })
            : Json(new { success = false, message = result.Message });
    }

    /// <summary>Yeni dil oluşturur .</summary>
    /// <param name="dto">Dil form verisi.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Dil", "Ekleme")]
    public async Task<IActionResult> Create(LanguageUpsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Dil adı zorunludur." });
        }

        var result = await languageService.CreateAsync(dto);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>Mevcut dili günceller .</summary>
    /// <param name="dto">Güncellenecek dil verisi.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Dil", "Güncelleme")]
    public async Task<IActionResult> Update(LanguageUpsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Dil adı zorunludur." });
        }

        var result = await languageService.UpdateAsync(dto);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>Bir dili siler .</summary>
    /// <param name="id">Silinecek dil kimliği.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Dil", "Silme")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await languageService.DeleteAsync(id);
        return Json(new { success = result.Success, message = result.Message });
    }
}
