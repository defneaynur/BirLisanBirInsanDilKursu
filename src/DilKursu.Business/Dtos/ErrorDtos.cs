namespace DilKursu.Business.Dtos;

/// <summary>
/// Yeni bir hata (exception) kaydı oluşturmak için gerekli veriyi taşıyan giriş DTO'su.
/// </summary>
public class ErrorLogEntryDto
{
    public string Module { get; set; } = "Sistem";

    public string? HttpMethod { get; set; }

    public string? Path { get; set; }

    public string ExceptionType { get; set; } = string.Empty;

    public string? Message { get; set; }

    public string? StackTrace { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public string? IpAddress { get; set; }
}

/// <summary>
/// Hata kaydını listeleme/görüntüleme için kullanılan çıktı DTO'su.
/// </summary>
public class ErrorLogDto
{
    public int Id { get; set; }

    public DateTime Timestamp { get; set; }

    public string Module { get; set; } = string.Empty;

    public string? HttpMethod { get; set; }

    public string? Path { get; set; }

    public string ExceptionType { get; set; } = string.Empty;

    public string? Message { get; set; }

    public string? StackTrace { get; set; }

    public string? UserName { get; set; }

    public string? IpAddress { get; set; }
}
