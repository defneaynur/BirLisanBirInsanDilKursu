using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DilKursu.Web.Controllers;

// Teknik hata (exception) kayıtlarını görüntüleme yalnızca sistem yöneticisine açıktır.
[Authorize(Roles = AppRoles.Admin)]
public class ErrorLogController(IErrorLogService errorLog) : Controller
{
    /// <summary>Hata kaydı listeleme sayfasını döndürür.</summary>
    [HttpGet]
    public IActionResult Index() => View();

    /// <summary>Hata kayıtlarını, isteğe bağlı modül filtresiyle JSON döndürür (DataTables kaynağı).</summary>
    /// <param name="module">Filtrelenecek modül (null/boş ise tümü).</param>
    [HttpGet]
    public async Task<IActionResult> List(string? module)
    {
        var result = await errorLog.GetAsync(module);
        return Json(result.Data);
    }
}
