namespace DilKursu.DataAccess.Identity;

public static class AppRoles
{
    /// <summary>
    /// Sistem yöneticisi rolü. Şube, derslik, dil, öğretmen tanımlama ve ders açma yetkilerine sahiptir.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Kayıt elemanı rolü. Öğrenci kaydı ve taksit tahsilatı işlemlerini yapar.
    /// </summary>
    public const string Kayit = "Kayit";

    /// <summary>Sistemde tanımlı tüm rollerin listesi (tohumlama için kullanılır).</summary>
    public static readonly string[] All = { Admin, Kayit };
}
