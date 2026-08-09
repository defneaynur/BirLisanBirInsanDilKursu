using DilKursu.Business.Common;
using DilKursu.Business.Dtos;

namespace DilKursu.Business.Services.Abstract;

public interface IClassroomService
{
    /// <summary>Tüm aktif derslikleri şube bilgisiyle birlikte döndürür.</summary>
    /// <returns>İşlem sonucu ve derslik listesi.</returns>
    Task<ServiceResult<IReadOnlyList<ClassroomDto>>> GetAllAsync();

    /// <summary>Belirli bir şubeye ait derslikleri döndürür.</summary>
    /// <param name="branchId">Şube kimliği.</param>
    /// <returns>İşlem sonucu ve derslik listesi.</returns>
    Task<ServiceResult<IReadOnlyList<ClassroomDto>>> GetByBranchAsync(int branchId);

    /// <summary>Idye göre tek bir dersliği döndürür.</summary>
    /// <param name="id">Derslik idsi.</param>
    /// <returns>İşlem sonucu ve derslik bilgisi.</returns>
    Task<ServiceResult<ClassroomDto>> GetByIdAsync(int id);

    /// <summary>Yeni bir derslik oluşturur.</summary>
    /// <param name="dto">Derslik verileri.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> CreateAsync(ClassroomUpsertDto dto);

    /// <summary>Derslik güncelleme işlemi gerçekleştirir.</summary>
    /// <param name="dto">Güncellenecek derslik verileri.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> UpdateAsync(ClassroomUpsertDto dto);

    /// <summary>Dersliği pasife alarak silme işlemini gerçekleştirir.</summary>
    /// <param name="id">Silinecek derslik idsi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> DeleteAsync(int id);
}
