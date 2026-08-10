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

    /// <summary>Son 6 ayda tahsil edilen (ödenmiş) taksit tutarlarını aya göre döndürür (gelir trendi).</summary>
    /// <returns>İşlem sonucu ve aylık tahsilat listesi.</returns>
    Task<ServiceResult<IReadOnlyList<NameAmountDto>>> GetMonthlyCollectionAsync();

    /// <summary>Son 6 ayda yapılan yeni kayıt sayılarını aya göre döndürür (büyüme trendi).</summary>
    /// <returns>İşlem sonucu ve aylık kayıt listesi.</returns>
    Task<ServiceResult<IReadOnlyList<NameCountDto>>> GetMonthlyEnrollmentsAsync();

    /// <summary>Haftanın günlerine göre açılmış ders sayısını döndürür (haftalık yoğunluk).</summary>
    /// <returns>İşlem sonucu ve gün bazlı ders sayısı listesi.</returns>
    Task<ServiceResult<IReadOnlyList<NameCountDto>>> GetWeeklyCourseDensityAsync();
}
