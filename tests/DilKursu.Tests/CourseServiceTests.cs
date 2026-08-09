using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Concrete;
using DilKursu.DataAccess.Context;
using DilKursu.DataAccess.UnitOfWork;
using DilKursu.Entities;
using DilKursu.Tests.TestSupport;
using Xunit;

namespace DilKursu.Tests;

public class CourseServiceTests
{
    // Test genelinde kullanılan sabit kimlikler ve zamanlar.
    private const int LanguageId = 1;
    private const int BranchId = 1;
    private const int ClassroomId = 1;
    private const int TeacherId = 1;

    /// <summary>
    /// Verilen bağlamı; bir dil, şube, derslik ve (İngilizce bilen, şubede çalışan,
    /// Pazartesi 09:00-18:00 müsait) bir öğretmen ile tohumlar.
    /// </summary>
    /// <param name="context">Tohumlanacak InMemory bağlam.</param>
    private static void SeedBaseData(AppDbContext context)
    {
        context.Languages.Add(new Language { Id = LanguageId, Name = "İngilizce" });

        var branch = new Branch { Id = BranchId, Name = "Merkez", Address = "Adres" };
        branch.Classrooms.Add(new Classroom { Id = ClassroomId, Name = "A-101", Capacity = 15, BranchId = BranchId });
        context.Branches.Add(branch);

        var teacher = new Teacher { Id = TeacherId, FullName = "Ayşe Öğretmen", StartDate = DateTime.Today };
        teacher.TeacherLanguages.Add(new TeacherLanguage { TeacherId = TeacherId, LanguageId = LanguageId });
        teacher.TeacherBranches.Add(new TeacherBranch { TeacherId = TeacherId, BranchId = BranchId });
        teacher.Availabilities.Add(new TeacherAvailability
        {
            TeacherId = TeacherId,
            Day = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(18, 0, 0)
        });
        context.Teachers.Add(teacher);

        context.SaveChanges();
    }

    /// <summary>
    /// Pazartesi 10:00-12:00 kriteri için, tüm koşulları sağlayan öğretmen ve boş dersliğin önerilmesi gerektiğini doğrular.
    /// </summary>
    [Fact]
    public async Task GetSuggestions_TumKosullarUygunsa_OgretmenVeDerslikOnerilir()
    {
        // Arrange: temel veriyi tohumla ve servisi kur.
        await using var context = InMemoryDbFactory.Create();
        SeedBaseData(context);
        var service = new CourseService(new UnitOfWork(context));

        var criteria = new CourseCriteriaDto
        {
            LanguageId = LanguageId,
            BranchId = BranchId,
            Day = DayOfWeek.Monday,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(12, 0, 0)
        };

        // Act
        var result = await service.GetSuggestionsAsync(criteria);

        // Assert: öğretmen ve derslik önerilmelidir.
        Assert.True(result.Success);
        Assert.Single(result.Data!.AvailableTeachers);
        Assert.Equal(TeacherId, result.Data!.AvailableTeachers[0].Id);
        Assert.Single(result.Data!.AvailableClassrooms);
    }

    /// <summary>
    /// Öğretmenin müsait olmadığı bir günde (Salı) hiçbir öğretmenin önerilmemesi gerektiğini doğrular.
    /// </summary>
    [Fact]
    public async Task GetSuggestions_OgretmenOGunMusaitDegilse_OgretmenOnerilmez()
    {
        await using var context = InMemoryDbFactory.Create();
        SeedBaseData(context);
        var service = new CourseService(new UnitOfWork(context));

        var criteria = new CourseCriteriaDto
        {
            LanguageId = LanguageId,
            BranchId = BranchId,
            Day = DayOfWeek.Tuesday, // Öğretmen yalnızca Pazartesi müsait.
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(12, 0, 0)
        };

        var result = await service.GetSuggestionsAsync(criteria);

        Assert.True(result.Success);
        Assert.Empty(result.Data!.AvailableTeachers);
    }

    /// <summary>
    /// İstenen dili bilmeyen (farklı dil için sorgulanan) öğretmenin önerilmemesi gerektiğini doğrular.
    /// </summary>
    [Fact]
    public async Task GetSuggestions_OgretmenDiliBilmiyorsa_OgretmenOnerilmez()
    {
        await using var context = InMemoryDbFactory.Create();
        SeedBaseData(context);
        // Farklı bir dil ekle (öğretmen bu dili bilmiyor).
        context.Languages.Add(new Language { Id = 2, Name = "Almanca" });
        context.SaveChanges();

        var service = new CourseService(new UnitOfWork(context));

        var criteria = new CourseCriteriaDto
        {
            LanguageId = 2, // Almanca — öğretmen bilmiyor.
            BranchId = BranchId,
            Day = DayOfWeek.Monday,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(12, 0, 0)
        };

        var result = await service.GetSuggestionsAsync(criteria);

        Assert.True(result.Success);
        Assert.Empty(result.Data!.AvailableTeachers);
    }

    /// <summary>
    /// Öğretmen aynı gün/saatte başka bir derse atanmışsa, çakışma nedeniyle önerilmemesi gerektiğini doğrular.
    /// Ayrıca o dersin işgal ettiği dersliğin de boş listelenmemesi gerekir.
    /// </summary>
    [Fact]
    public async Task GetSuggestions_OgretmenVeDerslikCakisiyorsa_Onerilmez()
    {
        await using var context = InMemoryDbFactory.Create();
        SeedBaseData(context);

        // Öğretmen ve dersliği Pazartesi 10:00-12:00 için başka bir derse ata (çakışma yarat).
        context.Courses.Add(new Course
        {
            Id = 99,
            LanguageId = LanguageId,
            BranchId = BranchId,
            TeacherId = TeacherId,
            ClassroomId = ClassroomId,
            Day = DayOfWeek.Monday,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(12, 0, 0),
            StartDate = DateTime.Today,
            Quota = 10,
            Fee = 1000
        });
        context.SaveChanges();

        var service = new CourseService(new UnitOfWork(context));

        var criteria = new CourseCriteriaDto
        {
            LanguageId = LanguageId,
            BranchId = BranchId,
            Day = DayOfWeek.Monday,
            StartTime = new TimeSpan(10, 30, 0), // Mevcut ders ile kesişiyor.
            EndTime = new TimeSpan(11, 30, 0)
        };

        var result = await service.GetSuggestionsAsync(criteria);

        Assert.True(result.Success);
        Assert.Empty(result.Data!.AvailableTeachers);   // Öğretmen çakışıyor.
        Assert.Empty(result.Data!.AvailableClassrooms); // Derslik dolu.
    }
}
