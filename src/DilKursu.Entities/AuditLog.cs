using DilKursu.Entities.Common;
using DilKursu.Entities.Enums;

namespace DilKursu.Entities;

/// <summary>
/// Kullanıcıların sistemde yaptığı işlemlerin denetim (audit) kaydını temsil eder.
/// Kayıtlar "[Modül][Aksiyon]" mantığıyla tutulur (ör. Modül="Ders", Action="Ekleme").
/// Denetim kaydı değiştirilmez; yalnızca eklenir ve listelenir.
/// Zaman damgası için <see cref="BaseEntity.CreatedDate"/> alanı kullanılır.
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>Kaydın önem seviyesi (Bilgi/Uyarı/Hata/Kritik).</summary>
    public AuditLevel Level { get; set; }

    /// <summary>İşlemin ait olduğu modül (ör. "Ders", "Öğrenci", "Kimlik").</summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>Yapılan işlem (ör. "Ekleme", "Güncelleme", "Silme", "Giriş").</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>İşleme dair açıklama/mesaj (ör. servis sonucu mesajı).</summary>
    public string? Message { get; set; }

    /// <summary>İşlemi yapan kullanıcının kimliği (giriş yapılmamışsa null).</summary>
    public string? UserId { get; set; }

    /// <summary>İşlemi yapan kullanıcının görünen adı/e-postası.</summary>
    public string? UserName { get; set; }

    /// <summary>İşlemin yapıldığı istemcinin IP adresi.</summary>
    public string? IpAddress { get; set; }
}
