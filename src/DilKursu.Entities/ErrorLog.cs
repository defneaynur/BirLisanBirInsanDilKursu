using DilKursu.Entities.Common;

namespace DilKursu.Entities;

/// <summary>
/// Sistemde oluşan yakalanmamış (beklenmeyen) hataların/istisnaların teknik kaydını temsil eder.
/// Kullanıcı işlemlerini tutan <see cref="AuditLog"/>'dan ayrı bir tabloda saklanır; çünkü hata
/// kayıtları farklı şemaya (yığın izi/stack trace), farklı hacme ve farklı saklama ömrüne sahiptir.
/// Zaman damgası için <see cref="BaseEntity.CreatedDate"/> alanı kullanılır.
/// </summary>
public class ErrorLog : BaseEntity
{
    /// <summary>Hatanın kaynaklandığı modül (ör. "Ders", "Öğrenci"); belirlenemezse "Sistem".</summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>İsteğin HTTP yöntemi (ör. "GET", "POST").</summary>
    public string? HttpMethod { get; set; }

    /// <summary>Hatanın oluştuğu kaynak (controller/action ya da ham istek yolu).</summary>
    public string? Path { get; set; }

    /// <summary>İstisnanın tür adı (ör. "SqlException", "NullReferenceException").</summary>
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>İstisna mesajı.</summary>
    public string? Message { get; set; }

    /// <summary>İstisnanın tam yığın izi (stack trace); teşhis için saklanır.</summary>
    public string? StackTrace { get; set; }

    /// <summary>Hata sırasında oturum açık olan kullanıcının kimliği (yoksa null).</summary>
    public string? UserId { get; set; }

    /// <summary>Hata sırasında oturum açık olan kullanıcının görünen adı/e-postası.</summary>
    public string? UserName { get; set; }

    /// <summary>İsteğin geldiği istemcinin IP adresi.</summary>
    public string? IpAddress { get; set; }
}
