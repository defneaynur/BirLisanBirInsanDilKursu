using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DilKursu.DataAccess.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    /// <summary>
    /// Şube tablosunun alan uzunluklarını, zorunluluklarını ve ilişkilerini yapılandırır.
    /// </summary>
    /// <param name="builder">Şube varlığı için yapılandırma oluşturucu.</param>
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).IsRequired().HasMaxLength(150);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(500);
        builder.Property(b => b.PublicTransportInstructions).HasMaxLength(1000);
        builder.Property(b => b.CarTransportInstructions).HasMaxLength(1000);
        builder.Property(b => b.SocialFacilities).HasMaxLength(1000);

        // Bir şube silinse dahi altındaki derslikler zincirleme silinmez; yetim kalmayı önlemek için Restrict.
        builder.HasMany(b => b.Classrooms)
               .WithOne(c => c.Branch)
               .HasForeignKey(c => c.BranchId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
