using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DilKursu.DataAccess.Configurations;

public class InstallmentConfiguration : IEntityTypeConfiguration<Installment>
{
    /// <summary>
    /// Taksit tablosunu ve tutar hassasiyetini yapılandırır.
    /// </summary>
    /// <param name="builder">Taksit varlığı için yapılandırma oluşturucu.</param>
    public void Configure(EntityTypeBuilder<Installment> builder)
    {
        builder.ToTable("Installments");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.SequenceNo).IsRequired();
        builder.Property(i => i.DueDate).IsRequired();
        builder.Property(i => i.Amount).HasPrecision(18, 2);
        builder.Property(i => i.IsPaid).IsRequired();
    }
}
