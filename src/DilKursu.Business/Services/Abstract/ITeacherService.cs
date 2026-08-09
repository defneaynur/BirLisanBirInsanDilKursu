using DilKursu.Business.Common;
using DilKursu.Business.Dtos;

namespace DilKursu.Business.Services.Abstract;

public interface ITeacherService
{
    /// <summary>Tüm aktif öğretmenleri dil ve şube özetleriyle getirir.</summary>
    /// <returns>İşlem sonucu ve öğretmen listesi.</returns>
    Task<ServiceResult<IReadOnlyList<TeacherDto>>> GetAllAsync();

    /// <summary>Idye göre öğretmeni, düzenleme için gerekli ilişkileriyle getirir.</summary>
    /// <param name="id">Öğretmen idsi.</param>
    /// <returns>İşlem sonucu ve öğretmen bilgisi.</returns>
    Task<ServiceResult<TeacherUpsertDto>> GetForEditAsync(int id);

    /// <summary>Diller, şubeler ve müsaitlikler ile birlikte yeni bir öğretmen oluşturur.</summary>
    /// <param name="dto">Öğretmen form verisi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> CreateAsync(TeacherUpsertDto dto);

    /// <summary>Öğretmen güncelleme işlemi gerçekleştirir.</summary>
    /// <param name="dto">Güncellenecek öğretmen verisi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> UpdateAsync(TeacherUpsertDto dto);

    /// <summary>Bir öğretmeni pasife alarak silme işlemini gerçekleştirir.</summary>
    /// <param name="id">Silinecek öğretmen idsi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> DeleteAsync(int id);
}
