using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DilKursu.Web.Infrastructure.Auditing;

namespace DilKursu.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class TeacherController(ITeacherService teacherService, ILanguageService languageService, IBranchService branchService) : Controller
{
    /// <summary>Öğretmen listeleme sayfasını döndürür.</summary>
    [HttpGet]
    public IActionResult Index() => View();

    /// <summary>Tüm öğretmenleri JSON olarak döndürür (DataTables kaynağı).</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await teacherService.GetAllAsync();
        return Json(result.Data);
    }

    /// <summary>Dil seçeneklerini JSON döndürür (form için).</summary>
    [HttpGet]
    public async Task<IActionResult> LanguageOptions()
    {
        var result = await languageService.GetAllAsync();
        return Json(result.Data?.Select(l => new { id = l.Id, name = l.Name }));
    }

    /// <summary>Şube seçeneklerini JSON döndürür (form için).</summary>
    [HttpGet]
    public async Task<IActionResult> BranchOptions()
    {
        var result = await branchService.GetAllAsync();
        return Json(result.Data?.Select(b => new { id = b.Id, name = b.Name }));
    }

    /// <summary>Tek bir öğretmeni düzenleme için (ilişkileriyle) JSON döndürür.</summary>
    /// <param name="id">Öğretmen kimliği.</param>
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var result = await teacherService.GetForEditAsync(id);
        return result.Success
            ? Json(new { success = true, data = result.Data })
            : Json(new { success = false, message = result.Message });
    }

    /// <summary>Yeni öğretmen oluşturur (AJAX POST, JSON gövde).</summary>
    /// <param name="dto">Öğretmen form verisi (diller, şubeler, müsaitlikler dahil).</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Öğretmen", "Ekleme")]
    public async Task<IActionResult> Create([FromBody] TeacherUpsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Lütfen zorunlu alanları doldurun." });
        }

        var result = await teacherService.CreateAsync(dto);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>Mevcut öğretmeni günceller.</summary>
    /// <param name="dto">Güncellenecek öğretmen verisi.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Öğretmen", "Güncelleme")]
    public async Task<IActionResult> Update([FromBody] TeacherUpsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Lütfen zorunlu alanları doldurun." });
        }

        var result = await teacherService.UpdateAsync(dto);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>Bir öğretmeni siler .</summary>
    /// <param name="id">Silinecek öğretmen kimliği.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Öğretmen", "Silme")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await teacherService.DeleteAsync(id);
        return Json(new { success = result.Success, message = result.Message });
    }
}
