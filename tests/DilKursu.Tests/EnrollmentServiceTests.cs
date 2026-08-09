using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Concrete;
using DilKursu.DataAccess.Context;
using DilKursu.DataAccess.UnitOfWork;
using DilKursu.Entities;
using DilKursu.Entities.Enums;
using DilKursu.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DilKursu.Tests;

public class EnrollmentServiceTests
{
    private const int StudentId = 1;
    private const int CourseId = 1;

    /// <summary>
    /// Bağlamı bir öğrenci ve verilen ücret/kontenjan ile bir ders içerecek şekilde tohumlar.
    /// </summary>
    /// <param name="context">Tohumlanacak bağlam.</param>
    /// <param name="fee">Ders ücreti.</param>
    /// <param name="quota">Ders kontenjanı.</param>
    private static void SeedStudentAndCourse(AppDbContext context, decimal fee, int quota)
    {
        context.Students.Add(new Student { Id = StudentId, FullName = "Test Öğrenci" });
        context.Languages.Add(new Language { Id = 1, Name = "İngilizce" });
        context.Branches.Add(new Branch { Id = 1, Name = "Merkez", Address = "Adres" });
        context.Classrooms.Add(new Classroom { Id = 1, Name = "A-101", Capacity = 15, BranchId = 1 });
        context.Teachers.Add(new Teacher { Id = 1, FullName = "Öğretmen", StartDate = DateTime.Today });
        context.Courses.Add(new Course
        {
            Id = CourseId,
            LanguageId = 1,
            BranchId = 1,
            TeacherId = 1,
            ClassroomId = 1,
            Day = DayOfWeek.Monday,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(12, 0, 0),
            StartDate = DateTime.Today,
            Quota = quota,
            Fee = fee
        });
        context.SaveChanges();
    }

    /// <summary>
    /// Peşin ödemede tam tutarlı tek bir taksit oluşturulması gerektiğini doğrular.
    /// </summary>
    [Fact]
    public async Task Enroll_PesinOdeme_TekTaksitOlusturur()
    {
        await using var context = InMemoryDbFactory.Create();
        SeedStudentAndCourse(context, fee: 6000m, quota: 10);
        var service = new EnrollmentService(new UnitOfWork(context));

        var result = await service.EnrollAsync(new EnrollmentCreateDto
        {
            StudentId = StudentId,
            CourseId = CourseId,
            PaymentType = OdemeTuru.Pesin,
            InstallmentCount = 1
        });

        Assert.True(result.Success);

        var installments = await context.Installments.ToListAsync();
        Assert.Single(installments);
        Assert.Equal(6000m, installments[0].Amount);
    }

    /// <summary>
    /// Taksitli ödemede istenen sayıda taksit oluşturulmasını ve taksit toplamının
    /// tam olarak toplam tutara eşit olmasını (yuvarlama artığı son taksitte) doğrular.
    /// </summary>
    [Fact]
    public async Task Enroll_TaksitliOdeme_TaksitleriOlusturur_ToplamKorunur()
    {
        await using var context = InMemoryDbFactory.Create();
        // 1000 / 3 = 333,33... — yuvarlama artığının doğru dağıtıldığı senaryo.
        SeedStudentAndCourse(context, fee: 1000m, quota: 10);
        var service = new EnrollmentService(new UnitOfWork(context));

        var result = await service.EnrollAsync(new EnrollmentCreateDto
        {
            StudentId = StudentId,
            CourseId = CourseId,
            PaymentType = OdemeTuru.Taksitli,
            InstallmentCount = 3
        });

        Assert.True(result.Success);

        var installments = await context.Installments.OrderBy(i => i.SequenceNo).ToListAsync();
        Assert.Equal(3, installments.Count);
        // Taksitlerin toplamı, ders ücretine tam eşit olmalıdır (kuruş kaybı olmamalı).
        Assert.Equal(1000m, installments.Sum(i => i.Amount));
    }

    /// <summary>
    /// Kontenjanı dolu bir derse yeni kaydın reddedilmesi gerektiğini doğrular.
    /// </summary>
    [Fact]
    public async Task Enroll_KontenjanDoluysa_BasarisizOlur()
    {
        await using var context = InMemoryDbFactory.Create();
        SeedStudentAndCourse(context, fee: 1000m, quota: 1);
        // Kontenjanı doldurmak için mevcut bir kayıt ekle.
        context.Enrollments.Add(new Enrollment
        {
            StudentId = 2,
            CourseId = CourseId,
            PaymentType = OdemeTuru.Pesin,
            TotalAmount = 1000m,
            EnrollmentDate = DateTime.Now
        });
        context.Students.Add(new Student { Id = 2, FullName = "Diğer Öğrenci" });
        context.SaveChanges();

        var service = new EnrollmentService(new UnitOfWork(context));

        var result = await service.EnrollAsync(new EnrollmentCreateDto
        {
            StudentId = StudentId,
            CourseId = CourseId,
            PaymentType = OdemeTuru.Pesin
        });

        Assert.False(result.Success);
        Assert.Contains("kontenjan", result.Message.ToLower());
    }

    /// <summary>
    /// Aynı öğrencinin aynı derse ikinci kez kaydedilememesi gerektiğini doğrular.
    /// </summary>
    [Fact]
    public async Task Enroll_AyniDerseIkinciKez_BasarisizOlur()
    {
        await using var context = InMemoryDbFactory.Create();
        SeedStudentAndCourse(context, fee: 1000m, quota: 10);
        var service = new EnrollmentService(new UnitOfWork(context));

        var dto = new EnrollmentCreateDto { StudentId = StudentId, CourseId = CourseId, PaymentType = OdemeTuru.Pesin };

        // İlk kayıt başarılı olmalı.
        var first = await service.EnrollAsync(dto);
        Assert.True(first.Success);

        // İkinci kayıt reddedilmeli.
        var second = await service.EnrollAsync(dto);
        Assert.False(second.Success);
    }

    /// <summary>
    /// Bir taksitin "ödendi" olarak işaretlenmesini ve kalan tutarın buna göre azalmasını doğrular.
    /// </summary>
    [Fact]
    public async Task PayInstallment_TaksitiOdendiIsaretler_KalanAzalir()
    {
        await using var context = InMemoryDbFactory.Create();
        SeedStudentAndCourse(context, fee: 900m, quota: 10);
        var service = new EnrollmentService(new UnitOfWork(context));

        await service.EnrollAsync(new EnrollmentCreateDto
        {
            StudentId = StudentId,
            CourseId = CourseId,
            PaymentType = OdemeTuru.Taksitli,
            InstallmentCount = 3
        });

        // İlk taksiti tahsil et.
        var firstInstallment = await context.Installments.OrderBy(i => i.SequenceNo).FirstAsync();
        var payResult = await service.PayInstallmentAsync(firstInstallment.Id);
        Assert.True(payResult.Success);

        // Öğrencinin kayıt ayrıntısını çek ve ödenen/kalan tutarları doğrula.
        var detail = await service.GetByStudentAsync(StudentId);
        var enrollment = Assert.Single(detail.Data!);
        Assert.Equal(300m, enrollment.PaidAmount);
        Assert.Equal(600m, enrollment.RemainingAmount);
    }

    /// <summary>
    /// Ödenmiş bir taksit için makbuz verisinin doğru üretildiğini (tutarlar, ödeme özeti) doğrular.
    /// </summary>
    [Fact]
    public async Task GetReceipt_OdenmisTaksit_DogruMakbuzUretir()
    {
        await using var context = InMemoryDbFactory.Create();
        SeedStudentAndCourse(context, fee: 6000m, quota: 10);
        var service = new EnrollmentService(new UnitOfWork(context));

        await service.EnrollAsync(new EnrollmentCreateDto
        {
            StudentId = StudentId,
            CourseId = CourseId,
            PaymentType = OdemeTuru.Taksitli,
            InstallmentCount = 3 // 3 x 2000
        });

        var first = await context.Installments.OrderBy(i => i.SequenceNo).FirstAsync();
        await service.PayInstallmentAsync(first.Id);

        var result = await service.GetReceiptAsync(first.Id);

        Assert.True(result.Success);
        Assert.Equal(2000m, result.Data!.InstallmentAmount);
        Assert.Equal(6000m, result.Data!.TotalAmount);
        Assert.Equal(2000m, result.Data!.PaidAmount);
        Assert.Equal(4000m, result.Data!.RemainingAmount);
        Assert.StartsWith("MKB-", result.Data!.ReceiptNo);
    }

    /// <summary>
    /// Ödenmemiş bir taksit için makbuz talebinin reddedildiğini doğrular.
    /// </summary>
    [Fact]
    public async Task GetReceipt_OdenmemisTaksit_BasarisizOlur()
    {
        await using var context = InMemoryDbFactory.Create();
        SeedStudentAndCourse(context, fee: 3000m, quota: 10);
        var service = new EnrollmentService(new UnitOfWork(context));

        await service.EnrollAsync(new EnrollmentCreateDto
        {
            StudentId = StudentId,
            CourseId = CourseId,
            PaymentType = OdemeTuru.Pesin
        });

        var installment = await context.Installments.FirstAsync();
        var result = await service.GetReceiptAsync(installment.Id);

        Assert.False(result.Success); // Peşin taksit henüz ödenmedi.
    }
}
