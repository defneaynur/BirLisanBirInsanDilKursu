namespace DilKursu.Entities.Enums;

/// <summary>
/// Kullanıcı işlem (audit) kayıtlarının önem/seviye derecesi.
/// Kayıtların filtrelenmesi ve renklendirilmesi bu seviyeye göre yapılır.
/// </summary>
public enum AuditLevel
{
    /// <summary>Normal, başarılı işlemler (ör. ders eklendi).</summary>
    Bilgi = 1,

    /// <summary>Engellenen ya da başarısız iş kuralı sonuçları (ör. kontenjan dolu).</summary>
    Uyari = 2,

    /// <summary>Beklenmeyen hata (istisna) sırasında oluşan kayıt.</summary>
    Hata = 3,

    /// <summary>Güvenlik veya sistem açısından kritik olaylar.</summary>
    Kritik = 4
}
