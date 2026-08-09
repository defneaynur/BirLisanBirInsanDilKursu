using DilKursu.Business.Common;
using DilKursu.Business.Dtos;

namespace DilKursu.Business.Services.Abstract;

public interface ICourseService
{
    /// <summary>Tüm aktif dersleri özet bilgileriyle döndürür.</summary>
    /// <returns>İşlem sonucu ve ders listesi.</returns>
    Task<ServiceResult<IReadOnlyList<CourseDto>>> GetAllAsync();

    /// <summary>
    /// Verilen kriterlere (dil, şube, gün, saat) göre uygun öğretmen ve boş derslik önerilerini üretir.
    /// Ders açma sihirbazının ikinci adımını besler.
    /// </summary>
    /// <param name="criteria">Sistem yöneticisinin girdiği ders açma kriterleri.</param>
    /// <returns>İşlem sonucu ve öneri verisi.</returns>
    Task<ServiceResult<CourseSuggestionDto>> GetSuggestionsAsync(CourseCriteriaDto criteria);

    /// <summary>
    /// Sihirbazda seçilen öğretmen ve derslik ile yeni bir ders oluşturur.
    /// Kaydetmeden önce öğretmen ve derslik uygunluğunu kontrol eder.
    /// </summary>
    /// <param name="dto">Ders oluşturma verisi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> CreateAsync(CourseCreateDto dto);

    /// <summary>Dersi pasife alarak silme işlemini gerçekleştirir.</summary>
    /// <param name="id">Silinecek ders idsi.</param>
    /// <returns>İşlem sonucu.</returns>
    Task<ServiceResult> DeleteAsync(int id);
}
