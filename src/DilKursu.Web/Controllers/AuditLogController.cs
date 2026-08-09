using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.Identity;
using DilKursu.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DilKursu.Web.Controllers;

// Denetim (audit) kayıtlarını görüntüleme yalnızca sistem yöneticisine açıktır.
[Authorize(Roles = AppRoles.Admin)]
public class AuditLogController(IAuditLogService auditLog) : Controller
{
    /// <summary>Denetim kaydı listeleme sayfasını döndürür.</summary>
    [HttpGet]
    public IActionResult Index() => View();

    /// <summary>Denetim kayıtlarını, isteğe bağlı seviye filtresiyle JSON döndürür (DataTables kaynağı).</summary>
    /// <param name="level">Filtrelenecek seviye (null ise tümü).</param>
    [HttpGet]
    public async Task<IActionResult> List(AuditLevel? level)
    {
        var result = await auditLog.GetAsync(level);
        return Json(result.Data);
    }
}
