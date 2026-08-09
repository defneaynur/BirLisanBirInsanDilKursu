using DilKursu.Business.Common;
using DilKursu.Business.Dtos;

namespace DilKursu.Business.Services.Abstract;

public interface IBranchService
{
    /// <summary>Tüm aktif şubeleri, derslik sayısı özetiyle birlikte döndürür.</summary>
    /// <returns>İşlem sonucu ve şube listesi.</returns>
    Task<ServiceResult<IReadOnlyList<BranchDto>>> GetAllAsync();

    /// <summary>Id ile tek bir şubeyi döndürür.</summary>
    /// <param name="id">Şube idsi.</param>
    /// <returns>İşlem sonucu ve şube bilgisi.</returns>
    Task<ServiceResult<BranchDto>> GetByIdAsync(int id);

    /// <summary>Yeni bir şube oluşturur.</summary>
    /// <param name="dto">Şube verileri.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> CreateAsync(BranchUpsertDto dto);

    /// <summary>Şube güncelleme işlemi gerçekleştirir.</summary>
    /// <param name="dto">Güncellenecek şube verisi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> UpdateAsync(BranchUpsertDto dto);

    /// <summary>Şubeyi pasife alarak silme işlemini gerçekleştirir.</summary>
    /// <param name="id">Silinecek şube idsi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> DeleteAsync(int id);
}
