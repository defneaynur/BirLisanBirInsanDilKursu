using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DilKursu.DataAccess.Configurations;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    /// <summary>
    /// Dil tablosunu yapılandırır; dil adını zorunlu ve benzersiz yapar.
    /// </summary>
    /// <param name="builder">Dil varlığı için yapılandırma oluşturucu.</param>
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name).IsRequired().HasMaxLength(80);

        // Aynı dilin iki kez tanımlanmasını engellemek için benzersiz indeks.
        builder.HasIndex(l => l.Name).IsUnique();
    }
}
