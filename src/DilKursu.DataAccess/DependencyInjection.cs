using DilKursu.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DilKursu.DataAccess;

public static class DependencyInjection
{
    /// <summary>
    /// EF Core <see cref="AppDbContext"/> bağlamını, verilen MSSQL bağlantı dizesiyle kaydeder.
    /// </summary>
    /// <param name="services">Servis koleksiyonu.</param>
    /// <param name="connectionString">MSSQL bağlantı dizesi.</param>
    /// <returns>Zincirleme kullanım için servis koleksiyonu.</returns>
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                // Migration'ların bu assembly içinde aranmasını sağlar.
                sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);

                // Geçici bağlantı hatalarına (ör. LocalDB soğuk başlatma gecikmesi) karşı
                // otomatik yeniden deneme: en fazla 5 deneme, aralarında en çok 10 sn bekleme.
                sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        return services;
    }
}
