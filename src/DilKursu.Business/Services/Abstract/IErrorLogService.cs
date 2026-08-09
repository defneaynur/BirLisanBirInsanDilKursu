using DilKursu.Business.Common;
using DilKursu.Business.Dtos;

namespace DilKursu.Business.Services.Abstract;

public interface IErrorLogService
{
    /// <summary>Yeni bir hata (exception) kaydı oluşturur.</summary>
    /// <param name="entry">Kaydedilecek hata verisi.</param>
    Task LogAsync(ErrorLogEntryDto entry);

    /// <summary>Hata kayıtlarını, isteğe bağlı modül filtresiyle en yeniden eskiye döndürür.</summary>
    /// <param name="module">Filtrelenecek modül (null/boş ise tümü).</param>
    /// <param name="take">Getirilecek azami kayıt sayısı.</param>
    /// <returns>İşlem sonucu ve hata kaydı listesi.</returns>
    Task<ServiceResult<IReadOnlyList<ErrorLogDto>>> GetAsync(string? module = null, int take = 500);
}
