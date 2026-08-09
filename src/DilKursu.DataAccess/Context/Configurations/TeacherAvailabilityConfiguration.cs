using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DilKursu.DataAccess.Configurations;

public class TeacherAvailabilityConfiguration : IEntityTypeConfiguration<TeacherAvailability>
{
    /// <summary>
    /// Öğretmen müsaitlik tablosunu yapılandırır.
    /// </summary>
    /// <param name="builder">Müsaitlik varlığı için yapılandırma oluşturucu.</param>
    public void Configure(EntityTypeBuilder<TeacherAvailability> builder)
    {
        builder.ToTable("TeacherAvailabilities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Day).IsRequired();
        builder.Property(a => a.StartTime).IsRequired();
        builder.Property(a => a.EndTime).IsRequired();
    }
}
