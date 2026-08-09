using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DilKursu.DataAccess.Configurations;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    /// <summary>
    /// Öğretmen tablosunu ve dil/şube/müsaitlik ilişkilerini yapılandırır.
    /// </summary>
    /// <param name="builder">Öğretmen varlığı için yapılandırma oluşturucu.</param>
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("Teachers");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.FullName).IsRequired().HasMaxLength(150);
        builder.Property(t => t.HomePhone).HasMaxLength(20);
        builder.Property(t => t.MobilePhone).HasMaxLength(20);
        builder.Property(t => t.StartDate).IsRequired();

        // Öğretmen silindiğinde öğretebildiği diller ve çalışabildiği şube bağlantıları da silinsin (Cascade).
        builder.HasMany(t => t.TeacherLanguages)
               .WithOne(tl => tl.Teacher)
               .HasForeignKey(tl => tl.TeacherId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.TeacherBranches)
               .WithOne(tb => tb.Teacher)
               .HasForeignKey(tb => tb.TeacherId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Availabilities)
               .WithOne(a => a.Teacher)
               .HasForeignKey(a => a.TeacherId)
               .OnDelete(DeleteBehavior.Cascade);

        // Öğretmen bir derse atalıysa silinemesin; ders kayıtlarının bütünlüğü için Restrict.
        builder.HasMany(t => t.Courses)
               .WithOne(c => c.Teacher)
               .HasForeignKey(c => c.TeacherId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
