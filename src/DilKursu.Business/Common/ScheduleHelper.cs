namespace DilKursu.Business.Common;

public static class ScheduleHelper
{
    /// <summary>
    /// Aynı gün içindeki iki zaman aralığının çakışıp çakışmadığını belirler.
    /// İki aralık; biri diğeri bitmeden başlıyorsa çakışır: (start1 &lt; end2) ve (start2 &lt; end1).
    /// Bitiş anının başlangıç anına eşit olması (art arda dersler) çakışma sayılmaz.
    /// </summary>
    /// <param name="start1">Birinci aralığın başlangıcı.</param>
    /// <param name="end1">Birinci aralığın bitişi.</param>
    /// <param name="start2">İkinci aralığın başlangıcı.</param>
    /// <param name="end2">İkinci aralığın bitişi.</param>
    /// <returns>Aralıklar çakışıyorsa true.</returns>
    public static bool Overlaps(TimeSpan start1, TimeSpan end1, TimeSpan start2, TimeSpan end2)
    {
        return start1 < end2 && start2 < end1;
    }

    /// <summary>
    /// Bir müsaitlik penceresinin, istenen ders saat aralığını tamamen kapsayıp kapsamadığını belirler.
    /// Öğretmenin dersi verebilmesi için müsaitlik başlangıcı ders başlangıcından erken/eşit,
    /// müsaitlik bitişi ders bitişinden geç/eşit olmalıdır.
    /// </summary>
    /// <param name="windowStart">Müsaitlik penceresi başlangıcı.</param>
    /// <param name="windowEnd">Müsaitlik penceresi bitişi.</param>
    /// <param name="requestedStart">İstenen ders başlangıcı.</param>
    /// <param name="requestedEnd">İstenen ders bitişi.</param>
    /// <returns>Pencere, istenen aralığı tamamen kapsıyorsa true.</returns>
    public static bool Covers(TimeSpan windowStart, TimeSpan windowEnd, TimeSpan requestedStart, TimeSpan requestedEnd)
    {
        return windowStart <= requestedStart && windowEnd >= requestedEnd;
    }
}
