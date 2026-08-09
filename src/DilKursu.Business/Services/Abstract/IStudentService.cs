using DilKursu.Business.Common;
using DilKursu.Business.Dtos;

namespace DilKursu.Business.Services.Abstract;

public interface IStudentService
{
    /// <summary>Tüm aktif öğrencileri kayıt sayısı özetiyle döndürür.</summary>
    /// <returns>İşlem sonucu ve öğrenci listesi.</returns>
    Task<ServiceResult<IReadOnlyList<StudentDto>>> GetAllAsync();

    /// <summary>Idye göre tek bir öğrenciyi döndürür.</summary>
    /// <param name="id">Öğrenci idsi.</param>
    /// <returns>İşlem sonucu ve öğrenci bilgisi.</returns>
    Task<ServiceResult<StudentDto>> GetByIdAsync(int id);

    /// <summary>Yeni bir öğrenci oluşturur.</summary>
    /// <param name="dto">Öğrenci form verisi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> CreateAsync(StudentUpsertDto dto);

    /// <summary>Öğrenci güncelleme işlemi gerçekleştirir.</summary>
    /// <param name="dto">Güncellenecek öğrenci verisi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> UpdateAsync(StudentUpsertDto dto);

    /// <summary>Bir öğrenciyi pasife alarak silme işlemini gerçekleştirir.</summary>
    /// <param name="id">Silinecek öğrenci idsi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> DeleteAsync(int id);
}
