using DilKursu.Business.Common;
using Xunit;

namespace DilKursu.Tests;

public class ScheduleHelperTests
{
    /// <summary>
    /// İç içe geçen veya kısmen kesişen aralıkların çakışma olarak tespit edilmesi gerektiğini doğrular.
    /// </summary>
    [Theory]
    [InlineData("09:00", "11:00", "10:00", "12:00")] // Kısmi kesişme
    [InlineData("09:00", "12:00", "10:00", "11:00")] // Biri diğerini kapsıyor
    [InlineData("10:00", "11:00", "09:00", "12:00")] // Ters kapsama
    public void Overlaps_KesisenAraliklar_TrueDoner(string s1, string e1, string s2, string e2)
    {
        // Verilen iki aralık kesiştiğinden çakışma beklenir.
        var result = ScheduleHelper.Overlaps(
            TimeSpan.Parse(s1), TimeSpan.Parse(e1), TimeSpan.Parse(s2), TimeSpan.Parse(e2));

        Assert.True(result);
    }

    /// <summary>
    /// Art arda gelen (uç uca değen) veya tamamen ayrık aralıkların çakışmaması gerektiğini doğrular.
    /// </summary>
    [Theory]
    [InlineData("09:00", "11:00", "11:00", "13:00")] // Uç uca (11:00 ortak sınır) — çakışma yok
    [InlineData("09:00", "10:00", "14:00", "15:00")] // Tamamen ayrık
    public void Overlaps_AyrikAraliklar_FalseDoner(string s1, string e1, string s2, string e2)
    {
        var result = ScheduleHelper.Overlaps(
            TimeSpan.Parse(s1), TimeSpan.Parse(e1), TimeSpan.Parse(s2), TimeSpan.Parse(e2));

        Assert.False(result);
    }

    /// <summary>
    /// Müsaitlik penceresinin, istenen ders aralığını tamamen kapsadığı durumu doğrular.
    /// </summary>
    [Fact]
    public void Covers_PencereAraligiKapsiyorsa_TrueDoner()
    {
        // 08:00-18:00 müsaitlik, 09:00-11:00 dersini kapsar.
        var result = ScheduleHelper.Covers(
            new TimeSpan(8, 0, 0), new TimeSpan(18, 0, 0),
            new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0));

        Assert.True(result);
    }

    /// <summary>
    /// İstenen ders, müsaitlik penceresinin dışına taştığında kapsamanın başarısız olduğunu doğrular.
    /// </summary>
    [Fact]
    public void Covers_DersPencereyiAsiyorsa_FalseDoner()
    {
        // 09:00-11:00 müsaitlik, 10:00-12:00 dersini kapsayamaz (12:00 > 11:00).
        var result = ScheduleHelper.Covers(
            new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0),
            new TimeSpan(10, 0, 0), new TimeSpan(12, 0, 0));

        Assert.False(result);
    }
}
