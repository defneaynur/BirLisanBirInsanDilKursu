using DilKursu.Business.Common;
using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.UnitOfWork;
using DilKursu.Entities;
using DilKursu.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace DilKursu.Business.Services.Concrete;

public class EnrollmentService(IUnitOfWork uow) : IEnrollmentService
{
    /// <summary>
    /// Öğrenciyi belirtilen derse kaydeder ve ödeme planını oluşturur.
    /// </summary>
    /// <param name="dto">Kayıt verileri.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> EnrollAsync(EnrollmentCreateDto dto)
    {
        // Öğrenci ve ders varlıklarının geçerliliği doğrulanır.
        var student = await uow.Students.GetByIdAsync(dto.StudentId);
        if (student is null)
        {
            return ServiceResult.Fail("Öğrenci bulunamadı.");
        }

        var course = await uow.Courses.Query()
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == dto.CourseId && c.IsActive);
        if (course is null)
        {
            return ServiceResult.Fail("Ders bulunamadı.");
        }

        // İş kuralı: öğrenci aynı derse birden fazla kez kaydedilemez.
        var alreadyEnrolled = await uow.Enrollments.AnyAsync(
            e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId);
        if (alreadyEnrolled)
        {
            return ServiceResult.Fail("Öğrenci bu derse zaten kayıtlı.");
        }

        // İş kuralı: kontenjan dolmuşsa yeni kayıt alınamaz.
        var activeEnrollmentCount = course.Enrollments.Count(e => e.IsActive);
        if (activeEnrollmentCount >= course.Quota)
        {
            return ServiceResult.Fail("Bu dersin kontenjanı dolmuştur.");
        }

        // Peşin ödemede taksit sayısı zorla 1 kabul edilir; taksitlide en az 2 olmalıdır.
        var installmentCount = dto.PaymentType == OdemeTuru.Pesin ? 1 : dto.InstallmentCount;
        if (dto.PaymentType == OdemeTuru.Taksitli && installmentCount < 2)
        {
            return ServiceResult.Fail("Taksitli ödemede taksit sayısı en az 2 olmalıdır.");
        }

        var enrollment = new Enrollment
        {
            StudentId = dto.StudentId,
            CourseId = dto.CourseId,
            EnrollmentDate = DateTime.Now,
            PaymentType = dto.PaymentType,
            TotalAmount = course.Fee
        };

        // Ödeme planına göre taksitler oluşturulur ve kayda eklenir.
        BuildInstallments(enrollment, installmentCount);

        await uow.Enrollments.AddAsync(enrollment);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Öğrenci derse başarıyla kaydedildi ve ödeme planı oluşturuldu.");
    }

    /// <summary>
    /// Belirtilen kaydın detaylarını döndürür.
    /// </summary>
    /// <param name="enrollmentId">Detayları alınacak kaydın ID'si.</param>
    /// <returns>İşlem sonucu ve kayıt detayları.</returns>
    public async Task<ServiceResult<EnrollmentDetailDto>> GetDetailAsync(int enrollmentId)
    {
        var enrollment = await LoadDetailQuery()
            .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.IsActive);

        if (enrollment is null)
        {
            return ServiceResult<EnrollmentDetailDto>.Fail("Kayıt bulunamadı.");
        }

        return ServiceResult<EnrollmentDetailDto>.Ok(MapToDetailDto(enrollment));
    }

    /// <summary>
    public async Task<ServiceResult<IReadOnlyList<EnrollmentDetailDto>>> GetByStudentAsync(int studentId)
    {
        var enrollments = await LoadDetailQuery()
            .Where(e => e.StudentId == studentId)
            .ToListAsync();

        var list = enrollments.Select(MapToDetailDto).ToList();
        return ServiceResult<IReadOnlyList<EnrollmentDetailDto>>.Ok(list);
    }

    /// <summary>
    /// Belirtilen taksiti ödenmiş olarak işaretler ve ödeme tarihini kaydeder. 
    /// </summary>
    /// <param name="installmentId">Ödenecek taksitin ID'si.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> PayInstallmentAsync(int installmentId)
    {
        var installment = await uow.Installments.GetByIdAsync(installmentId);
        if (installment is null)
        {
            return ServiceResult.Fail("Taksit bulunamadı.");
        }

        // İş kuralı: zaten ödenmiş bir taksit yeniden tahsil edilemez.
        if (installment.IsPaid)
        {
            return ServiceResult.Fail("Bu taksit zaten ödenmiş.");
        }

        installment.IsPaid = true;
        installment.PaidDate = DateTime.Now;

        uow.Installments.Update(installment);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Taksit ödendi olarak işaretlendi.");
    }

    /// <summary>
    /// Belirtilen taksitin makbuzunu döndürür.
    /// </summary>
    /// <param name="installmentId">Makbuzu alınacak taksitin ID'si.</param>
    /// <returns>İşlem sonucu ve makbuz bilgileri.</returns>
    public async Task<ServiceResult<ReceiptDto>> GetReceiptAsync(int installmentId)
    {
        // Taksit; öğrenci ve ders (dil/şube) bilgileriyle birlikte çekilir.
        // Not: Enrollment->Installments ilişkisi döngü oluşturacağından burada yüklenmez;
        // ödenen toplam aşağıda ayrı bir sorguyla hesaplanır.
        var installment = await uow.Installments.Query()
            .Include(i => i.Enrollment).ThenInclude(e => e.Student)
            .Include(i => i.Enrollment).ThenInclude(e => e.Course).ThenInclude(c => c.Language)
            .Include(i => i.Enrollment).ThenInclude(e => e.Course).ThenInclude(c => c.Branch)
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == installmentId);

        if (installment is null)
        {
            return ServiceResult<ReceiptDto>.Fail("Taksit bulunamadı.");
        }

        // İş kuralı: yalnızca tahsil edilmiş taksit için makbuz düzenlenebilir.
        if (!installment.IsPaid)
        {
            return ServiceResult<ReceiptDto>.Fail("Yalnızca ödenmiş taksitler için makbuz düzenlenebilir.");
        }

        var enrollment = installment.Enrollment;

        // Bu kayda ait ödenen taksitlerin toplamı (döngüyü önlemek için ayrı sorgu).
        var paid = await uow.Installments.Query()
            .Where(i => i.EnrollmentId == enrollment.Id && i.IsPaid)
            .SumAsync(i => i.Amount);

        var receipt = new ReceiptDto
        {
            // Makbuz numarası ödeme yılı ve taksit kimliğinden üretilir.
            ReceiptNo = $"MKB-{(installment.PaidDate ?? DateTime.Now):yyyy}-{installment.Id:D6}",
            IssueDate = DateTime.Now,
            StudentName = enrollment.Student.FullName,
            StudentPhone = enrollment.Student.MobilePhone,
            CourseInfo = $"{enrollment.Course.Language.Name} - {enrollment.Course.Level} ({enrollment.Course.Branch.Name})",
            PaymentType = enrollment.PaymentType,
            InstallmentNo = installment.SequenceNo,
            InstallmentAmount = installment.Amount,
            PaidDate = installment.PaidDate,
            TotalAmount = enrollment.TotalAmount,
            PaidAmount = paid,
            RemainingAmount = enrollment.TotalAmount - paid
        };

        return ServiceResult<ReceiptDto>.Ok(receipt);
    }

    /// <summary>
    /// Ödeme türüne ve taksit sayısına göre kayda ait taksitleri oluşturur.
    /// Tutar taksitlere eşit bölünür; kuruş farkı (yuvarlama artığı) son taksite eklenir.
    /// Peşin ödemede tek taksitin vadesi kayıt günüdür; taksitlide vadeler birer ay arayla ilerler.
    /// </summary>
    /// <param name="enrollment">Taksitlerin ekleneceği kayıt.</param>
    /// <param name="installmentCount">Oluşturulacak taksit sayısı.</param>
    private static void BuildInstallments(Enrollment enrollment, int installmentCount)
    {
        // Taksit başına düşen temel tutar 2 ondalık basamağa yuvarlanır.
        var baseAmount = Math.Round(enrollment.TotalAmount / installmentCount, 2, MidpointRounding.AwayFromZero);

        // İlk (n-1) taksitin toplamı; son taksit kalan tutarı üstlenerek toplamı korur.
        var runningTotal = 0m;

        for (var i = 1; i <= installmentCount; i++)
        {
            var isLast = i == installmentCount;
            var amount = isLast ? enrollment.TotalAmount - runningTotal : baseAmount;
            runningTotal += amount;

            enrollment.Installments.Add(new Installment
            {
                SequenceNo = i,
                // Peşinde bugün, taksitlide her taksit için bir ay sonrası vade.
                DueDate = enrollment.PaymentType == OdemeTuru.Pesin
                    ? enrollment.EnrollmentDate.Date
                    : enrollment.EnrollmentDate.Date.AddMonths(i - 1),
                Amount = amount,
                IsPaid = false
            });
        }
    }

    /// <summary>
    /// Kayıt ayrıntısı için gerekli tüm ilişkileri yükleyen ortak sorguyu döndürür.
    /// </summary>
    /// <returns>Öğrenci, ders (dil/şube) ve taksitleri içeren sorgu.</returns>
    private IQueryable<Enrollment> LoadDetailQuery()
    {
        return uow.Enrollments.Query()
            .Include(e => e.Student)
            .Include(e => e.Course).ThenInclude(c => c.Language)
            .Include(e => e.Course).ThenInclude(c => c.Branch)
            .Include(e => e.Installments)
            .AsNoTracking();
    }

    /// <summary>
    /// Bir Enrollment entity'sini, ödeme özeti ve taksit listesini içeren
    /// EnrollmentDetailDto'ya dönüştürür.
    /// </summary>
    /// <param name="enrollment">Kaynak kayıt entity'si.</param>
    /// <returns>Eşlenmiş ayrıntı DTO'su.</returns>
    private static EnrollmentDetailDto MapToDetailDto(Enrollment enrollment)
    {
        var installments = enrollment.Installments
            .Where(i => i.IsActive)
            .OrderBy(i => i.SequenceNo)
            .Select(i => new InstallmentDto
            {
                Id = i.Id,
                SequenceNo = i.SequenceNo,
                DueDate = i.DueDate,
                Amount = i.Amount,
                IsPaid = i.IsPaid,
                PaidDate = i.PaidDate
            }).ToList();

        var paid = installments.Where(i => i.IsPaid).Sum(i => i.Amount);

        return new EnrollmentDetailDto
        {
            Id = enrollment.Id,
            StudentName = enrollment.Student.FullName,
            CourseInfo = $"{enrollment.Course.Language.Name} - {enrollment.Course.Level} ({enrollment.Course.Branch.Name})",
            PaymentType = enrollment.PaymentType,
            TotalAmount = enrollment.TotalAmount,
            PaidAmount = paid,
            RemainingAmount = enrollment.TotalAmount - paid,
            Installments = installments
        };
    }
}
