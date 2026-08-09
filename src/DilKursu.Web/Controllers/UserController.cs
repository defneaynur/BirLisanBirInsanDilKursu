using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DilKursu.Web.Infrastructure.Auditing;

namespace DilKursu.Web.Controllers;

// Kullanıcı (personel) yönetimi yalnızca sistem yöneticisine açıktır.
[Authorize(Roles = AppRoles.Admin)]
public class UserController(IUserService userService, IBranchService branchService) : Controller
{
    /// <summary>Kullanıcı listeleme sayfasını döndürür.</summary>
    [HttpGet]
    public IActionResult Index() => View();

    /// <summary>Tüm kullanıcıları JSON olarak döndürür (DataTables kaynağı).</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await userService.GetAllAsync();
        return Json(result.Data);
    }

    /// <summary>Formdaki şube açılır kutusunu doldurmak için şube seçeneklerini JSON döndürür.</summary>
    [HttpGet]
    public async Task<IActionResult> BranchOptions()
    {
        var result = await branchService.GetAllAsync();
        return Json(result.Data?.Select(b => new { id = b.Id, name = b.Name }));
    }

    /// <summary>Tek bir kullanıcıyı düzenleme için JSON döndürür.</summary>
    /// <param name="id">Kullanıcı idsi.</param>
    [HttpGet]
    public async Task<IActionResult> Get(string id)
    {
        var result = await userService.GetForEditAsync(id);
        return result.Success
            ? Json(new { success = true, data = result.Data })
            : Json(new { success = false, message = result.Message });
    }

    /// <summary>Yeni kullanıcı oluşturur .</summary>
    /// <param name="dto">Kullanıcı form verisi.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Kullanıcı", "Ekleme")]
    public async Task<IActionResult> Create(UserUpsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Lütfen zorunlu alanları doldurun." });
        }

        var result = await userService.CreateAsync(dto);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>Mevcut kullanıcıyı günceller .</summary>
    /// <param name="dto">Güncellenecek kullanıcı verisi.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Kullanıcı", "Güncelleme")]
    public async Task<IActionResult> Update(UserUpsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Lütfen zorunlu alanları doldurun." });
        }

        var result = await userService.UpdateAsync(dto);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>Bir kullanıcıyı siler .</summary>
    /// <param name="id">Silinecek kullanıcı idsi.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Kullanıcı", "Silme")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await userService.DeleteAsync(id);
        return Json(new { success = result.Success, message = result.Message });
    }
}
