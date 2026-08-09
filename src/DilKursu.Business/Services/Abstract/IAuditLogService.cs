using DilKursu.Business.Common;
using DilKursu.Business.Dtos;
using DilKursu.Entities.Enums;

namespace DilKursu.Business.Services.Abstract;

public interface IAuditLogService
{
    /// <summary>Yeni bir denetim (audit) kaydı oluşturur.</summary>
    /// <param name="entry">Kaydedilecek denetim verisi.</param>
    Task LogAsync(AuditEntryDto entry);

    /// <summary>Denetim kayıtlarını, isteğe bağlı seviye/modül filtresiyle en yeniden eskiye döndürür.</summary>
    /// <param name="level">Filtrelenecek seviye (null ise tümü).</param>
    /// <param name="module">Filtrelenecek modül (null/boş ise tümü).</param>
    /// <param name="take">Getirilecek azami kayıt sayısı.</param>
    /// <returns>İşlem sonucu ve denetim kaydı listesi.</returns>
    Task<ServiceResult<IReadOnlyList<AuditLogDto>>> GetAsync(AuditLevel? level = null, string? module = null, int take = 500);
}
