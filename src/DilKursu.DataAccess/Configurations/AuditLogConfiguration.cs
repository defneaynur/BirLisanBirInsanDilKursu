using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DilKursu.DataAccess.Configurations;

/// <summary>
/// <see cref="AuditLog"/> varlığının veritabanı eşlemesini (tablo, alanlar, indeksler) tanımlar.
/// Denetim tablosunda sık sorgu yapılan alanlara (tarih, seviye, modül) indeks eklenir.
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    /// <summary>
    /// Denetim kaydı tablosunu yapılandırır.
    /// </summary>
    /// <param name="builder">AuditLog varlığı için yapılandırma oluşturucu.</param>
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Level).IsRequired();
        builder.Property(a => a.Module).IsRequired().HasMaxLength(60);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(60);
        builder.Property(a => a.Message).HasMaxLength(1000);
        builder.Property(a => a.UserId).HasMaxLength(450);
        builder.Property(a => a.UserName).HasMaxLength(256);
        builder.Property(a => a.IpAddress).HasMaxLength(64);

        // Listeleme/filtreleme sorguları için indeksler.
        builder.HasIndex(a => a.CreatedDate);
        builder.HasIndex(a => a.Level);
        builder.HasIndex(a => a.Module);
    }
}
