using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DilKursu.DataAccess.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    /// <summary>
    /// Ders tablosunu, ücret hassasiyetini ve zorunlu ilişkileri yapılandırır.
    /// </summary>
    /// <param name="builder">Ders varlığı için yapılandırma oluşturucu.</param>
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Level).IsRequired();
        builder.Property(c => c.Day).IsRequired();
        builder.Property(c => c.StartTime).IsRequired();
        builder.Property(c => c.EndTime).IsRequired();
        builder.Property(c => c.StartDate).IsRequired();
        builder.Property(c => c.Quota).IsRequired();

        // Parasal alanlar için ondalık hassasiyeti (18 basamak, 2 ondalık) sabitlenir.
        builder.Property(c => c.Fee).HasPrecision(18, 2);

        builder.HasOne(c => c.Language)
               .WithMany(l => l.Courses)
               .HasForeignKey(c => c.LanguageId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Branch)
               .WithMany(b => b.Courses)
               .HasForeignKey(c => c.BranchId)
               .OnDelete(DeleteBehavior.Restrict);

        // Bir ders silindiğinde ona ait öğrenci kayıtları da silinsin (Cascade).
        builder.HasMany(c => c.Enrollments)
               .WithOne(e => e.Course)
               .HasForeignKey(e => e.CourseId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
