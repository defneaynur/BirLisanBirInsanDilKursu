using DilKursu.Business.Common;
using DilKursu.Business.Dtos;

namespace DilKursu.Business.Services.Abstract;

public interface ILanguageService
{
    /// <summary>Tüm aktif dilleri getirir.</summary>
    /// <returns>İşlem sonucu ve dil listesi.</returns>
    Task<ServiceResult<IReadOnlyList<LanguageDto>>> GetAllAsync();

    /// <summary>Idye göre tek bir dili döndürür.</summary>
    /// <param name="id">Dil idsi.</param>
    /// <returns>İşlem sonucu ve dil bilgisi.</returns>
    Task<ServiceResult<LanguageDto>> GetByIdAsync(int id);

    /// <summary>Yeni bir dil oluşturur.</summary>
    /// <param name="dto">Dil form verisi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> CreateAsync(LanguageUpsertDto dto);

    /// <summary>Dil güncelleme işlemi gerçekleştirir.</summary>
    /// <param name="dto">Güncellenecek dil verisi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> UpdateAsync(LanguageUpsertDto dto);

    /// <summary>Bir dili pasife alarak silme işlemini gerçekleştirir.</summary>
    /// <param name="id">Silinecek dil idsi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> DeleteAsync(int id);
}
