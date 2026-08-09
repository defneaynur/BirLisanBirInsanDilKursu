using System.Reflection;
using System.Security.Claims;
using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DilKursu.Web.Infrastructure.Auditing;

/// <summary>
/// <see cref="AuditAttribute"/> ile işaretlenmiş action'lar çalıştıktan sonra otomatik denetim kaydı
/// oluşturan merkezî action filter. İşlemin sonucundan (AJAX yanıtındaki <c>success</c> alanı veya
/// oluşan istisna) hareketle uygun seviye (Bilgi/Uyarı/Hata) belirlenir. Böylece her action içinde
/// tek tek loglama çağrısı yapmaya gerek kalmaz (DRY, kesişen ilgi/AOP yaklaşımı).
/// </summary>
/// <param name="audit">Denetim kaydı iş servisi.</param>
public class AuditActionFilter(IAuditLogService audit) : IAsyncActionFilter
{
    /// <summary>Action'ı çalıştırır ve işaretliyse sonucuna göre denetim kaydı oluşturur.</summary>
    /// <param name="context">Action yürütme bağlamı.</param>
    /// <param name="next">Sonraki filtreyi/action'ı çalıştıran temsilci.</param>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Önce action (ve varsa sonraki filtreler) çalıştırılır.
        var executed = await next();

        // Action üzerinde [Audit] özniteliği yoksa hiçbir şey yapılmaz.
        var attribute = (context.ActionDescriptor as ControllerActionDescriptor)?
            .MethodInfo.GetCustomAttribute<AuditAttribute>();
        if (attribute is null)
        {
            return;
        }

        // Action bir istisna fırlattıysa buraya denetim kaydı YAZILMAZ; hata, ayrı ErrorLogs tablosuna
        // ErrorLoggingMiddleware tarafından (yığın izi dahil) kaydedilir. İstisna üst katmana bırakılır.
        if (executed.Exception is not null)
        {
            return;
        }

        // İşlemin başarı durumu ve mesajı (AJAX yanıtından) belirlenir.
        var success = true;
        string? message = null;

        if (executed.Result is JsonResult { Value: { } value })
        {
            // AJAX yanıtı genelde { success, message } yapısındadır; yansıma (reflection) ile okunur.
            if (value.GetType().GetProperty("success")?.GetValue(value) is bool s)
            {
                success = s;
            }
            message = value.GetType().GetProperty("message")?.GetValue(value)?.ToString();
        }

        // Sonuca göre seviye: başarısız iş kuralı → Uyarı, başarılı → Bilgi.
        var level = success ? AuditLevel.Bilgi : AuditLevel.Uyari;

        var user = context.HttpContext.User;

        await audit.LogAsync(new AuditEntryDto
        {
            Level = level,
            Module = attribute.Module,
            Action = attribute.Action,
            Message = message,
            UserId = user.FindFirstValue(ClaimTypes.NameIdentifier),
            UserName = user.Identity?.Name,
            IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString()
        });
    }
}
