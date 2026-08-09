using DilKursu.Business.Common;
using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.UnitOfWork;
using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DilKursu.Business.Services.Concrete;

/// <param name="uow">Hata kaydı deposuna erişim için veri erişim işlem yöneticisi.</param>
/// <param name="logger">Kayıtları dosyaya/konsola da düşürmek için kod tarafı loglayıcı (Serilog).</param>
public class ErrorLogService(IUnitOfWork uow, ILogger<ErrorLogService> logger) : IErrorLogService
{
    /// <summary>Yeni bir hata (exception) kaydı oluşturur; DB yazımı başarısız olsa dahi isteği bozmaz.</summary>
    /// <param name="entry">Kaydedilecek hata verisi.</param>
    public async Task LogAsync(ErrorLogEntryDto entry)
    {
        try
        {
            // Alanlar, veritabanı sütun sınırlarını aşan durumlarda insert'in patlamaması için kırpılır.
            var log = new ErrorLog
            {
                Module = Truncate(entry.Module, 60)!,
                HttpMethod = Truncate(entry.HttpMethod, 10),
                Path = Truncate(entry.Path, 500),
                ExceptionType = Truncate(entry.ExceptionType, 200)!,
                Message = Truncate(entry.Message, 2000),
                StackTrace = entry.StackTrace, // nvarchar(max): kırpılmaz
                UserId = Truncate(entry.UserId, 450),
                UserName = Truncate(entry.UserName, 256),
                IpAddress = Truncate(entry.IpAddress, 64)
            };

            await uow.ErrorLogs.AddAsync(log);
            await uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Hata kaydı veritabanına yazılamazsa uygulama akışı kesilmez; durum koda loglanır.
            logger.LogError(ex, "Hata kaydı veritabanına yazılamadı: [{Module}] {ExceptionType}",
                entry.Module, entry.ExceptionType);
        }
    }

    /// <summary>Hata kayıtlarını, isteğe bağlı modül filtresiyle en yeniden eskiye döndürür.</summary>
    /// <param name="module">Filtrelenecek modül (null/boş ise tümü).</param>
    /// <param name="take">Getirilecek azami kayıt sayısı.</param>
    /// <returns>İşlem sonucu ve hata kaydı listesi.</returns>
    public async Task<ServiceResult<IReadOnlyList<ErrorLogDto>>> GetAsync(string? module = null, int take = 500)
    {
        var query = uow.ErrorLogs.Query();

        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(e => e.Module == module);
        }

        var logs = await query
            .OrderByDescending(e => e.CreatedDate)
            .Take(take)
            .AsNoTracking()
            .Select(e => new ErrorLogDto
            {
                Id = e.Id,
                Timestamp = e.CreatedDate,
                Module = e.Module,
                HttpMethod = e.HttpMethod,
                Path = e.Path,
                ExceptionType = e.ExceptionType,
                Message = e.Message,
                StackTrace = e.StackTrace,
                UserName = e.UserName,
                IpAddress = e.IpAddress
            })
            .ToListAsync();

        return ServiceResult<IReadOnlyList<ErrorLogDto>>.Ok(logs);
    }

    /// <summary>Metni verilen azami uzunluğa güvenli biçimde kırpar (null ise null döner).</summary>
    /// <param name="value">Kırpılacak metin.</param>
    /// <param name="maxLength">İzin verilen azami karakter sayısı.</param>
    /// <returns>Sınıra uyacak şekilde kırpılmış metin.</returns>
    private static string? Truncate(string? value, int maxLength) =>
        value is not null && value.Length > maxLength ? value[..maxLength] : value;
}
