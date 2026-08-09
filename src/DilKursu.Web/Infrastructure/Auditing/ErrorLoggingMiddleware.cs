using System.Reflection;
using System.Security.Claims;
using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace DilKursu.Web.Infrastructure.Auditing;

/// <summary>
/// İstek işleme hattında yakalanmamış (beklenmeyen) sistem hatalarını yakalayıp <b>ErrorLogs</b>
/// tablosuna teknik hata kaydı (yığın izi dahil) olarak yazan ara katman (middleware). Bu kayıtlar,
/// kullanıcı işlemlerini tutan denetim (audit) tablosundan ayrıdır. Hatanın <b>hangi modülden</b>
/// geldiği, isteği karşılayan controller'dan çözülür: action'ın kendi <see cref="AuditAttribute"/>
/// modülü varsa o kullanılır, yoksa controller adı Türkçe modül etiketine eşlenir, hiçbiri yoksa
/// "Sistem" yazılır. Hatayı kaydettikten sonra <b>yeniden fırlatır</b>; böylece geliştirme/üretim
/// hata sayfası davranışı hiç değişmez.
/// </summary>
/// <param name="next">Hattaki bir sonraki bileşeni çalıştıran temsilci.</param>
/// <param name="logger">Kod tarafı loglayıcı (Serilog).</param>
public class ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
{
    /// <summary>Controller adı → Türkçe modül etiketi eşlemesi (denetim kayıtlarındaki modül adlarıyla aynı).</summary>
    private static readonly Dictionary<string, string> ControllerModules = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Branch"] = "Şube",
        ["Classroom"] = "Derslik",
        ["Language"] = "Dil",
        ["Teacher"] = "Öğretmen",
        ["Student"] = "Öğrenci",
        ["Course"] = "Ders",
        ["Enrollment"] = "Kayıt",
        ["User"] = "Kullanıcı",
        ["Account"] = "Kimlik",
        ["Report"] = "Rapor",
        ["AuditLog"] = "Denetim",
        ["ErrorLog"] = "Hata",
        ["Home"] = "Panel"
    };

    /// <summary>İsteği çalıştırır; beklenmeyen bir istisna oluşursa hata kaydı bırakır ve yeniden fırlatır.</summary>
    /// <param name="context">Geçerli HTTP istek bağlamı.</param>
    /// <param name="errorLog">Hata kaydı iş servisi (istek kapsamından çözülür).</param>
    public async Task InvokeAsync(HttpContext context, IErrorLogService errorLog)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // İsteği karşılayan controller/action, hatanın kaynağını (modülü) belirlemek için okunur.
            var descriptor = context.GetEndpoint()?.Metadata.GetMetadata<ControllerActionDescriptor>();
            var module = ResolveModule(descriptor);
            var source = descriptor is null
                ? context.Request.Path.ToString()
                : $"{descriptor.ControllerName}/{descriptor.ActionName}";

            // Kod tarafı: tam yığın izi (stack trace) ile dosyaya/konsola loglanır.
            logger.LogError(ex, "Beklenmeyen sistem hatası: [{Module}] {Method} {Source}",
                module, context.Request.Method, source);

            // DB tarafı: kaynağa (modül), kullanıcıya ve yığın izine bağlanabilir teknik hata kaydı bırakılır.
            var user = context.User;
            await errorLog.LogAsync(new ErrorLogEntryDto
            {
                Module = module,
                HttpMethod = context.Request.Method,
                Path = source,
                ExceptionType = ex.GetType().Name,
                Message = ex.Message,
                StackTrace = ex.ToString(),
                UserId = user.FindFirstValue(ClaimTypes.NameIdentifier),
                UserName = user.Identity?.Name,
                IpAddress = context.Connection.RemoteIpAddress?.ToString()
            });

            // Hata sayfası/geliştirici sayfası akışının çalışması için istisna yeniden fırlatılır.
            throw;
        }
    }

    /// <summary>Hatanın geldiği modülü, action'ın [Audit] özniteliğinden ya da controller adından çözer.</summary>
    /// <param name="descriptor">İsteği karşılayan controller/action tanımı (yoksa null).</param>
    /// <returns>Türkçe modül etiketi; belirlenemezse "Sistem".</returns>
    private static string ResolveModule(ControllerActionDescriptor? descriptor)
    {
        if (descriptor is null)
        {
            // MVC dışı bir uç nokta (statik dosya, ara katman vb.) için genel "Sistem" etiketi.
            return "Sistem";
        }

        // 1) Action'ın kendi [Audit] modülü varsa en doğru kaynak odur.
        var audit = descriptor.MethodInfo.GetCustomAttribute<AuditAttribute>();
        if (audit is not null)
        {
            return audit.Module;
        }

        // 2) Aksi halde controller adından bilinen modüle eşle; tanınmıyorsa "Sistem".
        return ControllerModules.GetValueOrDefault(descriptor.ControllerName, "Sistem");
    }
}
