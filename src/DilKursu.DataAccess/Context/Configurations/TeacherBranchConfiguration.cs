using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DilKursu.DataAccess.Configurations;

public class TeacherBranchConfiguration : IEntityTypeConfiguration<TeacherBranch>
{
    /// <summary>
    /// Öğretmen-Şube bağlantı tablosunu yapılandırır; aynı öğretmen-şube çiftinin tekrarını engeller.
    /// </summary>
    /// <param name="builder">Bağlantı varlığı için yapılandırma oluşturucu.</param>
    public void Configure(EntityTypeBuilder<TeacherBranch> builder)
    {
        builder.ToTable("TeacherBranches");

        builder.HasKey(tb => tb.Id);

        // Bir öğretmene aynı şube iki kez eklenemesin.
        builder.HasIndex(tb => new { tb.TeacherId, tb.BranchId }).IsUnique();

        builder.HasOne(tb => tb.Branch)
               .WithMany(b => b.TeacherBranches)
               .HasForeignKey(tb => tb.BranchId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
