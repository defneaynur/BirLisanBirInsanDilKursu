using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DilKursu.Web.Infrastructure.Auditing;

namespace DilKursu.Web.Controllers;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Kayit)]
public class BranchController(IBranchService branchService) : Controller
{
    /// <summary>
    /// Şube listeleme sayfasını (DataTables barındıran görünüm) döndürür.
    /// </summary>
    [HttpGet]
    public IActionResult Index() => View();

    /// <summary>
    /// Tüm şubeleri JSON olarak döndürür (DataTables'ın AJAX kaynağı).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await branchService.GetAllAsync();
        return Json(result.Data);
    }

    /// <summary>
    /// Tek bir şubeyi düzenleme formu için JSON olarak döndürür.
    /// </summary>
    /// <param name="id">Şube kimliği.</param>
    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var result = await branchService.GetByIdAsync(id);
        if (!result.Success)
        {
            return Json(new { success = false, message = result.Message });
        }

        return Json(new { success = true, data = result.Data });
    }

    /// <summary>
    /// Yeni şube oluşturur . Sonuç, SweetAlert2 için standart yapıda döner.
    /// </summary>
    /// <param name="dto">Şube form verisi.</param>
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    [Audit("Şube", "Ekleme")]
    public async Task<IActionResult> Create(BranchUpsertDto dto)
    {
        // Sunucu tarafı doğrulama; istemci doğrulaması atlansa bile korunur.
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Lütfen zorunlu alanları doldurun." });
        }

        var result = await branchService.CreateAsync(dto);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>
    /// Mevcut şubeyi günceller .
    /// </summary>
    /// <param name="dto">Güncellenecek şube verisi.</param>
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    [Audit("Şube", "Güncelleme")]
    public async Task<IActionResult> Update(BranchUpsertDto dto)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Lütfen zorunlu alanları doldurun." });
        }

        var result = await branchService.UpdateAsync(dto);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>
    /// Bir şubeyi siler .
    /// </summary>
    /// <param name="id">Silinecek şube kimliği.</param>
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ValidateAntiForgeryToken]
    [Audit("Şube", "Silme")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await branchService.DeleteAsync(id);
        return Json(new { success = result.Success, message = result.Message });
    }
}
