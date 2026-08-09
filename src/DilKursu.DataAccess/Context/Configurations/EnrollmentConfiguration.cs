using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DilKursu.DataAccess.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    /// <summary>
    /// Kayıt tablosunu, toplam tutar hassasiyetini ve taksit ilişkisini yapılandırır.
    /// </summary>
    /// <param name="builder">Kayıt varlığı için yapılandırma oluşturucu.</param>
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PaymentType).IsRequired();
        builder.Property(e => e.EnrollmentDate).IsRequired();
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);

        // Bir kayıt silindiğinde ona ait taksitler de silinsin (Cascade).
        builder.HasMany(e => e.Installments)
               .WithOne(i => i.Enrollment)
               .HasForeignKey(i => i.EnrollmentId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
