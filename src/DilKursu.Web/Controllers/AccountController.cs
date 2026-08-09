using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.Identity;
using DilKursu.Entities.Enums;
using DilKursu.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DilKursu.Web.Controllers;

public class AccountController(SignInManager<ApplicationUser> signInManager, IAuditLogService auditLog) : Controller
{
    /// <summary>
    /// Giriş formunu görüntüler. Kullanıcı zaten giriş yapmışsa ana sayfaya yönlendirir.
    /// </summary>
    /// <param name="returnUrl">Giriş sonrası dönülecek adres.</param>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    /// <summary>
    /// Giriş formunu işler; kimlik bilgilerini doğrular ve başarılıysa oturumu başlatır.
    /// </summary>
    /// <param name="model">Giriş form verisi.</param>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // Model doğrulaması başarısızsa aynı form geri gösterilir.
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // E-posta ile parola doğrulaması yapılır (kilitlenme desteği açık).
        var result = await signInManager.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            await LogAuthAsync(AuditLevel.Bilgi, "Giriş", "Giriş başarılı.", model.Email);

            // Açık redirect saldırılarına karşı yalnızca yerel adreslere yönlendirilir.
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            await LogAuthAsync(AuditLevel.Kritik, "Giriş", "Hesap kilitlendi (çok sayıda hatalı deneme).", model.Email);
            ModelState.AddModelError(string.Empty, "Hesabınız çok sayıda hatalı deneme nedeniyle geçici olarak kilitlendi.");
            return View(model);
        }

        // Başarısız giriş denemesi de güvenlik açısından denetime kaydedilir.
        await LogAuthAsync(AuditLevel.Uyari, "Giriş", "Hatalı giriş denemesi.", model.Email);

        // Genel hata mesajı (kullanıcı sızıntısını önlemek için ayrıntı verilmez).
        ModelState.AddModelError(string.Empty, "E-posta veya parola hatalı.");
        return View(model);
    }

    /// <summary>
    /// Kullanıcının oturumunu kapatır ve giriş sayfasına yönlendirir.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // Çıkıştan önce kullanıcı adı yakalanır (oturum kapandıktan sonra erişilemez).
        var userName = User.Identity?.Name;
        await signInManager.SignOutAsync();
        await LogAuthAsync(AuditLevel.Bilgi, "Çıkış", "Oturum kapatıldı.", userName);
        return RedirectToAction("Login", "Account");
    }

    /// <summary>Kimlik (giriş/çıkış) işlemleri için denetim kaydı oluşturur.</summary>
    /// <param name="level">Kaydın önem seviyesi.</param>
    /// <param name="action">İşlem adı (ör. "Giriş", "Çıkış").</param>
    /// <param name="message">Açıklama mesajı.</param>
    /// <param name="userName">İşlemi yapan kullanıcının adı/e-postası.</param>
    private Task LogAuthAsync(AuditLevel level, string action, string message, string? userName) =>
        auditLog.LogAsync(new AuditEntryDto
        {
            Level = level,
            Module = "Kimlik",
            Action = action,
            Message = message,
            UserName = userName,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

    /// <summary>
    /// Yetkisiz erişim durumunda gösterilen "erişim reddedildi" sayfası.
    /// </summary>
    [HttpGet]
    [Authorize]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
