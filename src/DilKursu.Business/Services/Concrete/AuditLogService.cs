using DilKursu.Business.Common;
using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.UnitOfWork;
using DilKursu.Entities;
using DilKursu.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DilKursu.Business.Services.Concrete;

/// <param name="uow">Denetim kaydı deposuna erişim için veri erişim işlem yöneticisi.</param>
/// <param name="logger">Kayıtları dosyaya/konsola da düşürmek için kod tarafı loglayıcı (Serilog).</param>
public class AuditLogService(IUnitOfWork uow, ILogger<AuditLogService> logger) : IAuditLogService
{
    /// <summary>Yeni bir denetim (audit) kaydı oluşturur; DB yazımı başarısız olsa dahi isteği bozmaz.</summary>
    /// <param name="entry">Kaydedilecek denetim verisi.</param>
    public async Task LogAsync(AuditEntryDto entry)
    {
        // Kod tarafı loglama: her denetim olayı yapılandırılmış (structured) olarak dosyaya/konsola düşer.
        logger.Log(MapToLogLevel(entry.Level),
            "AUDIT [{Module}][{Action}] {Level} | Kullanıcı: {User} | {Message}",
            entry.Module, entry.Action, entry.Level, entry.UserName ?? "-", entry.Message ?? "-");

        try
        {
            // Alanlar, veritabanı sütun sınırlarını aşan (ör. uzun istisna mesajı) durumlarda
            // insert'in patlamaması için güvenli biçimde kırpılır.
            var log = new AuditLog
            {
                Level = entry.Level,
                Module = Truncate(entry.Module, 60)!,
                Action = Truncate(entry.Action, 60)!,
                Message = Truncate(entry.Message, 1000),
                UserId = Truncate(entry.UserId, 450),
                UserName = Truncate(entry.UserName, 256),
                IpAddress = Truncate(entry.IpAddress, 64)
            };

            await uow.AuditLogs.AddAsync(log);
            await uow.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Denetim kaydı veritabanına yazılamazsa uygulama akışı kesilmez; hata koda loglanır.
            logger.LogError(ex, "Denetim kaydı veritabanına yazılamadı: [{Module}][{Action}]",
                entry.Module, entry.Action);
        }
    }

    /// <summary>Denetim kayıtlarını, isteğe bağlı seviye/modül filtresiyle en yeniden eskiye döndürür.</summary>
    /// <param name="level">Filtrelenecek seviye (null ise tümü).</param>
    /// <param name="module">Filtrelenecek modül (null/boş ise tümü).</param>
    /// <param name="take">Getirilecek azami kayıt sayısı.</param>
    /// <returns>İşlem sonucu ve denetim kaydı listesi.</returns>
    public async Task<ServiceResult<IReadOnlyList<AuditLogDto>>> GetAsync(AuditLevel? level = null, string? module = null, int take = 500)
    {
        var query = uow.AuditLogs.Query();

        if (level.HasValue)
        {
            query = query.Where(a => a.Level == level.Value);
        }

        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(a => a.Module == module);
        }

        var logs = await query
            .OrderByDescending(a => a.CreatedDate)
            .Take(take)
            .AsNoTracking()
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                Timestamp = a.CreatedDate,
                Level = a.Level,
                Module = a.Module,
                Action = a.Action,
                Message = a.Message,
                UserName = a.UserName,
                IpAddress = a.IpAddress
            })
            .ToListAsync();

        return ServiceResult<IReadOnlyList<AuditLogDto>>.Ok(logs);
    }

    /// <summary>Metni verilen azami uzunluğa güvenli biçimde kırpar (null ise null döner).</summary>
    /// <param name="value">Kırpılacak metin.</param>
    /// <param name="maxLength">İzin verilen azami karakter sayısı.</param>
    /// <returns>Sınıra uyacak şekilde kırpılmış metin.</returns>
    private static string? Truncate(string? value, int maxLength) =>
        value is not null && value.Length > maxLength ? value[..maxLength] : value;

    /// <summary>Denetim seviyesini, kod tarafı loglama seviyesine (Serilog) eşler.</summary>
    /// <param name="level">Denetim seviyesi.</param>
    /// <returns>Karşılık gelen loglama seviyesi.</returns>
    private static Microsoft.Extensions.Logging.LogLevel MapToLogLevel(AuditLevel level) => level switch
    {
        AuditLevel.Bilgi => Microsoft.Extensions.Logging.LogLevel.Information,
        AuditLevel.Uyari => Microsoft.Extensions.Logging.LogLevel.Warning,
        AuditLevel.Hata => Microsoft.Extensions.Logging.LogLevel.Error,
        AuditLevel.Kritik => Microsoft.Extensions.Logging.LogLevel.Critical,
        _ => Microsoft.Extensions.Logging.LogLevel.Information
    };
}
