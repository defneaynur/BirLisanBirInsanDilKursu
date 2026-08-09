using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DilKursu.DataAccess.Configurations;

/// <summary>
/// <see cref="ErrorLog"/> varlığının veritabanı eşlemesini (tablo, alanlar, indeksler) tanımlar.
/// Yığın izi (stack trace) uzun olabildiğinden sınırsız (nvarchar(max)) tutulur; sık sorgulanan
/// alanlara (tarih, modül, istisna türü) indeks eklenir.
/// </summary>
public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    /// <summary>Hata kaydı tablosunu yapılandırır.</summary>
    /// <param name="builder">ErrorLog varlığı için yapılandırma oluşturucu.</param>
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.ToTable("ErrorLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Module).IsRequired().HasMaxLength(60);
        builder.Property(e => e.HttpMethod).HasMaxLength(10);
        builder.Property(e => e.Path).HasMaxLength(500);
        builder.Property(e => e.ExceptionType).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Message).HasMaxLength(2000);
        // Yığın izi çok uzun olabilir; uzunluk sınırı konmaz (nvarchar(max)).
        builder.Property(e => e.UserId).HasMaxLength(450);
        builder.Property(e => e.UserName).HasMaxLength(256);
        builder.Property(e => e.IpAddress).HasMaxLength(64);

        // Listeleme/filtreleme sorguları için indeksler.
        builder.HasIndex(e => e.CreatedDate);
        builder.HasIndex(e => e.Module);
        builder.HasIndex(e => e.ExceptionType);
    }
}
