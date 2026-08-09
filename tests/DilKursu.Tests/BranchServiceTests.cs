using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Concrete;
using DilKursu.DataAccess.UnitOfWork;
using DilKursu.Entities;
using DilKursu.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DilKursu.Tests;

public class BranchServiceTests
{
    /// <summary>
    /// Geçerli veriyle yeni bir şubenin oluşturulabildiğini ve kalıcılaştığını doğrular.
    /// </summary>
    [Fact]
    public async Task Create_GecerliVeri_SubeOlusturur()
    {
        await using var context = InMemoryDbFactory.Create();
        var service = new BranchService(new UnitOfWork(context));

        var result = await service.CreateAsync(new BranchUpsertDto
        {
            Name = "Kadıköy",
            Address = "Moda Cad. No:1"
        });

        Assert.True(result.Success);
        Assert.Equal(1, await context.Branches.CountAsync());
    }

    /// <summary>
    /// Aynı isimde ikinci bir şube oluşturmanın benzersizlik kuralıyla engellendiğini doğrular.
    /// </summary>
    [Fact]
    public async Task Create_AyniIsim_BasarisizOlur()
    {
        await using var context = InMemoryDbFactory.Create();
        var service = new BranchService(new UnitOfWork(context));

        await service.CreateAsync(new BranchUpsertDto { Name = "Merkez", Address = "Adres 1" });
        var second = await service.CreateAsync(new BranchUpsertDto { Name = "Merkez", Address = "Adres 2" });

        Assert.False(second.Success);
        Assert.Equal(1, await context.Branches.CountAsync());
    }

    /// <summary>
    /// İçinde açılmış ders bulunan bir şubenin silinemediğini (veri bütünlüğü) doğrular.
    /// </summary>
    [Fact]
    public async Task Delete_SubedeDersVarsa_BasarisizOlur()
    {
        await using var context = InMemoryDbFactory.Create();

        var branch = new Branch { Id = 1, Name = "Merkez", Address = "Adres" };
        context.Branches.Add(branch);
        context.Languages.Add(new Language { Id = 1, Name = "İngilizce" });
        context.Classrooms.Add(new Classroom { Id = 1, Name = "A-101", Capacity = 10, BranchId = 1 });
        context.Teachers.Add(new Teacher { Id = 1, FullName = "Öğretmen", StartDate = DateTime.Today });
        context.Courses.Add(new Course
        {
            Id = 1,
            LanguageId = 1,
            BranchId = 1,
            TeacherId = 1,
            ClassroomId = 1,
            Day = DayOfWeek.Monday,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(12, 0, 0),
            StartDate = DateTime.Today,
            Quota = 10,
            Fee = 1000
        });
        context.SaveChanges();

        var service = new BranchService(new UnitOfWork(context));

        var result = await service.DeleteAsync(1);

        Assert.False(result.Success);
    }
}
