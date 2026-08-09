using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DilKursu.DataAccess.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    /// <summary>
    /// Öğrenci tablosunu ve kayıt ilişkisini yapılandırır.
    /// </summary>
    /// <param name="builder">Öğrenci varlığı için yapılandırma oluşturucu.</param>
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.FullName).IsRequired().HasMaxLength(150);
        builder.Property(s => s.HomePhone).HasMaxLength(20);
        builder.Property(s => s.MobilePhone).HasMaxLength(20);

        // Öğrenci silindiğinde kayıtları da silinsin (Cascade); taksitler kayda bağlı olarak devam eder.
        builder.HasMany(s => s.Enrollments)
               .WithOne(e => e.Student)
               .HasForeignKey(e => e.StudentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
