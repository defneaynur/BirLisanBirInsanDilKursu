using DilKursu.Business.Common;
using DilKursu.Business.Dtos;

namespace DilKursu.Business.Services.Abstract;

public interface IReportService
{
    /// <summary>Tüm aktif derslerin doluluk durumunu döndürür.</summary>
    /// <returns>İşlem sonucu ve ders doluluk listesi.</returns>
    Task<ServiceResult<IReadOnlyList<CourseOccupancyDto>>> GetCourseOccupancyAsync();

    /// <summary>Şube başına açılmış ders sayısını döndürür (dağılım grafiği için).</summary>
    /// <returns>İşlem sonucu ve şube dağılım listesi.</returns>
    Task<ServiceResult<IReadOnlyList<NameCountDto>>> GetBranchCourseDistributionAsync();

    /// <summary>Dil başına açılmış ders sayısını döndürür (dağılım grafiği için).</summary>
    /// <returns>İşlem sonucu ve dil dağılım listesi.</returns>
    Task<ServiceResult<IReadOnlyList<NameCountDto>>> GetLanguageCourseDistributionAsync();
}
