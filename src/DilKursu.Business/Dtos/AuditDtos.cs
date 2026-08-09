using DilKursu.Entities.Enums;

namespace DilKursu.Business.Dtos;

/// <summary>
/// Yeni bir denetim kaydı oluşturmak için gerekli veriyi taşıyan giriş DTO'su.
/// </summary>
public class AuditEntryDto
{
    public AuditLevel Level { get; set; } = AuditLevel.Bilgi;

    public string Module { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? Message { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string? IpAddress { get; set; }
}

/// <summary>
/// Denetim kaydını listeleme/görüntüleme için kullanılan çıktı DTO'su.
/// </summary>
public class AuditLogDto
{
    public int Id { get; set; }

    public DateTime Timestamp { get; set; }

    public AuditLevel Level { get; set; }

    public string Module { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public string? Message { get; set; }

    public string? UserName { get; set; }

    public string? IpAddress { get; set; }
}
