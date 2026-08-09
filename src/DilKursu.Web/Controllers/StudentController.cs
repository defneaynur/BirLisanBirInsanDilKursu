using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DilKursu.Web.Infrastructure.Auditing;

namespace DilKursu.Web.Controllers;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Kayit)]
public class StudentController(IStudentService studentService) : Controller
{
    /// <summary>Öğrenci listeleme sayfasını döndürür.</summary>
    [HttpGet]
    public IActionResult Index() => View();

    /// <summary>Tüm öğrencileri JSON olarak döndürür (DataTables kaynağı).</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await studentService.GetAllAsync();
        return Json(result.Data);
    }

    /// <summary>Tek bir öğrenciyi düzenleme için JSON döndürür.</summary>
    /// <param name="id">Öğrenci kimliği.</param>
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var result = await studentService.GetByIdAsync(id);
        return result.Success
            ? Json(new { success = true, data = result.Data })
            : Json(new { success = false, message = result.Message });
    }

    /// <summary>Yeni öğrenci oluşturur .</summary>
    /// <param name="dto">Öğrenci form verisi.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Öğrenci", "Ekleme")]
    public async Task<IActionResult> Create(StudentUpsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Ad soyad zorunludur." });
        }

        var result = await studentService.CreateAsync(dto);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>Mevcut öğrenciyi günceller .</summary>
    /// <param name="dto">Güncellenecek öğrenci verisi.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Öğrenci", "Güncelleme")]
    public async Task<IActionResult> Update(StudentUpsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Ad soyad zorunludur." });
        }

        var result = await studentService.UpdateAsync(dto);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>Bir öğrenciyi siler .</summary>
    /// <param name="id">Silinecek öğrenci kimliği.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Öğrenci", "Silme")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await studentService.DeleteAsync(id);
        return Json(new { success = result.Success, message = result.Message });
    }
}
