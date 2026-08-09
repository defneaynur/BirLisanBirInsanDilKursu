using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DilKursu.Web.Infrastructure.Auditing;

namespace DilKursu.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
public class ClassroomController(IClassroomService classroomService, IBranchService branchService) : Controller
{
    /// <summary>Derslik listeleme sayfasını döndürür.</summary>
    [HttpGet]
    public IActionResult Index() => View();

    /// <summary>Tüm derslikleri JSON olarak döndürür (DataTables kaynağı).</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await classroomService.GetAllAsync();
        return Json(result.Data);
    }

    /// <summary>
    /// Form açılış kutusunu doldurmak için şube seçeneklerini (id, ad) JSON döndürür.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> BranchOptions()
    {
        var result = await branchService.GetAllAsync();
        var options = result.Data?.Select(b => new { id = b.Id, name = b.Name });
        return Json(options);
    }

    /// <summary>Tek bir dersliği düzenleme için JSON döndürür.</summary>
    /// <param name="id">Derslik kimliği.</param>
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var result = await classroomService.GetByIdAsync(id);
        return result.Success
            ? Json(new { success = true, data = result.Data })
            : Json(new { success = false, message = result.Message });
    }

    /// <summary>Yeni derslik oluşturur.</summary>
    /// <param name="dto">Derslik form verisi.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Derslik", "Ekleme")]
    public async Task<IActionResult> Create(ClassroomUpsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Lütfen zorunlu alanları doldurun." });
        }

        var result = await classroomService.CreateAsync(dto);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>Mevcut dersliği günceller .</summary>
    /// <param name="dto">Güncellenecek derslik verisi.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Derslik", "Güncelleme")]
    public async Task<IActionResult> Update(ClassroomUpsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Lütfen zorunlu alanları doldurun." });
        }

        var result = await classroomService.UpdateAsync(dto);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>Bir dersliği siler.</summary>
    /// <param name="id">Silinecek derslik kimliği.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Derslik", "Silme")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await classroomService.DeleteAsync(id);
        return Json(new { success = result.Success, message = result.Message });
    }
}
