using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DilKursu.DataAccess.Configurations;

public class ClassroomConfiguration : IEntityTypeConfiguration<Classroom>
{
    /// <summary>
    /// Derslik tablosunun alanlarını ve ders ilişkisini yapılandırır.
    /// </summary>
    /// <param name="builder">Derslik varlığı için yapılandırma oluşturucu.</param>
    public void Configure(EntityTypeBuilder<Classroom> builder)
    {
        builder.ToTable("Classrooms");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Capacity).IsRequired();

        // Bir derslik silindiğinde o derslikteki dersler otomatik silinmesin; veri tutarlılığı için Restrict.
        builder.HasMany(c => c.Courses)
               .WithOne(co => co.Classroom)
               .HasForeignKey(co => co.ClassroomId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
