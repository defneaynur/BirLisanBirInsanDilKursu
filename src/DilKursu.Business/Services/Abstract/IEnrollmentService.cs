using DilKursu.Business.Common;
using DilKursu.Business.Dtos;

namespace DilKursu.Business.Services.Abstract;

public interface IEnrollmentService
{
    /// <summary>
    /// Bir öğrenciyi bir derse kaydeder ve ödeme türüne göre taksitleri oluşturur.
    /// </summary>
    /// <param name="dto">Kayıt ve ödeme planı verisi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> EnrollAsync(EnrollmentCreateDto dto);

    /// <summary>Bir kaydın ödeme durumunu (taksitler dahil) ayrıntılı döndürür.</summary>
    /// <param name="enrollmentId">Kayıt kimliği.</param>
    /// <returns>İşlem sonucu ve kayıt ödeme bilgisi.</returns>
    Task<ServiceResult<EnrollmentDetailDto>> GetDetailAsync(int enrollmentId);

    /// <summary>Bir öğrencinin tüm kayıtlarını, ödeme özetleriyle listeler.</summary>
    /// <param name="studentId">Öğrenci idsi.</param>
    /// <returns>İşlem sonucu ve kayıt listesi.</returns>
    Task<ServiceResult<IReadOnlyList<EnrollmentDetailDto>>> GetByStudentAsync(int studentId);

    /// <summary>
    /// Belirli bir taksiti tahsil edilmiş (ödendi) olarak işaretler.
    /// </summary>
    /// <param name="installmentId">Ödenen taksitin idsi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> PayInstallmentAsync(int installmentId);

    /// <summary>
    /// Ödenmiş bir taksit için makbuz verisini hazırlar.
    /// Yalnızca ödenmiş taksitler için makbuz düzenlenebilir.
    /// </summary>
    /// <param name="installmentId">Makbuzu istenen (ödenmiş) taksitin idsi.</param>
    /// <returns>İşlem sonucu ve makbuz verisi.</returns>
    Task<ServiceResult<ReceiptDto>> GetReceiptAsync(int installmentId);
}
