using DilKursu.Business.Common;
using DilKursu.Business.Dtos;

namespace DilKursu.Business.Services.Abstract;


public interface IUserService
{
    /// <summary>
    /// Tüm aktif userları döndürür.
    /// </summary>
    /// <returns>İşlem sonucu ve user listesi.</returns>
    Task<ServiceResult<IReadOnlyList<AppUserDto>>> GetAllAsync();

    /// <summary>
    /// Belirli bir userı düzenleme için döndürür.
    /// </summary>
    /// <param name="id">User idsi.</param>
    /// <returns>İşlem sonucu ve user bilgisi.</returns>
    Task<ServiceResult<UserUpsertDto>> GetForEditAsync(string id);

    /// <summary>
    /// Yeni bir user oluşturur.
    /// </summary>
    /// <param name="dto">User verileri.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> CreateAsync(UserUpsertDto dto);

    /// <summary>
    /// Mevcut bir userı günceller.
    /// </summary>
    /// <param name="dto">Güncellenecek user verileri.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> UpdateAsync(UserUpsertDto dto);

    /// <summary>
    /// Bir userı pasife alarak silme işlemini gerçekleştirir.
    /// </summary>
    /// <param name="id">Silinecek user idsi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> DeleteAsync(string id);
}
