namespace DilKursu.Business.Common;

public class ServiceResult<T>
{
    public bool Success { get; private set; }

    public string Message { get; private set; } = string.Empty;

    public T? Data { get; private set; }

    /// <summary>
    /// Dışarıdan doğrudan örnek oluşturulmasını engeller; nesneler fabrika metotlarıyla üretilir.
    /// </summary>
    private ServiceResult() { }

    /// <summary>
    ///  Başarılı bir sonuç döndürür.
    /// </summary>
    /// <param name="data">Dönen veri.</param>
    /// <param name="message">Dönen mesaj. Default "İşlem başarılı" döner.</param>
    /// <returns>Başarılı sonuç nesnesi.</returns>
    public static ServiceResult<T> Ok(T data, string message = "İşlem başarılı.")
        => new() { Success = true, Data = data, Message = message };

    /// <summary>
    /// Başarısız bir sonuç döndürür.
    /// </summary>
    /// <param name="message">Hata mesajı.</param>
    /// <returns>Başarısız sonuç datası.</returns>
    public static ServiceResult<T> Fail(string message)
        => new() { Success = false, Message = message, Data = default };
}

public class ServiceResult
{
    public bool Success { get; private set; }

    public string Message { get; private set; } = string.Empty;

    private ServiceResult() { }

    /// <summary>
    ///  Başarılı bir sonuç döndürür.
    /// </summary>
    /// <param name="message">Dönen mesaj. Default "İşlem başarılı" döner.</param>
    /// <returns>Başarılı sonuç nesnesi.</returns>
    public static ServiceResult Ok(string message = "İşlem başarılı.")
        => new() { Success = true, Message = message };

    /// <summary>
    ///  Başarısız bir sonuç döndürür.
    /// </summary>
    /// <param name="message">Hata mesajı.</param>
    /// <returns>Başarısız sonuç nesnesi.</returns>
    public static ServiceResult Fail(string message)
        => new() { Success = false, Message = message };
}
