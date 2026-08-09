using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DilKursu.DataAccess.Configurations;

public class TeacherLanguageConfiguration : IEntityTypeConfiguration<TeacherLanguage>
{
    /// <summary>
    /// Öğretmen-Dil bağlantı tablosunu yapılandırır; aynı öğretmen-dil çiftinin tekrarını engeller.
    /// </summary>
    /// <param name="builder">Bağlantı varlığı için yapılandırma oluşturucu.</param>
    public void Configure(EntityTypeBuilder<TeacherLanguage> builder)
    {
        builder.ToTable("TeacherLanguages");

        builder.HasKey(tl => tl.Id);

        // Bir öğretmene aynı dil iki kez eklenemesin.
        builder.HasIndex(tl => new { tl.TeacherId, tl.LanguageId }).IsUnique();

        builder.HasOne(tl => tl.Language)
               .WithMany(l => l.TeacherLanguages)
               .HasForeignKey(tl => tl.LanguageId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
