using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.Identity;
using DilKursu.Web.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DilKursu.Web.Infrastructure.Auditing;
using QuestPDF.Fluent;

namespace DilKursu.Web.Controllers;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.Kayit)]
public class EnrollmentController(IEnrollmentService enrollmentService, IStudentService studentService, ICourseService courseService) : Controller
{
    /// <summary>Kayıt (enrollment) sayfasını döndürür.</summary>
    [HttpGet]
    public IActionResult Index() => View();

    /// <summary>Öğrenci seçeneklerini JSON döndürür (kayıt formu için).</summary>
    [HttpGet]
    public async Task<IActionResult> StudentOptions()
    {
        var result = await studentService.GetAllAsync();
        return Json(result.Data?.Select(s => new { id = s.Id, name = s.FullName }));
    }

    /// <summary>
    /// Ders seçeneklerini JSON döndürür (kayıt formu için).
    /// Herhangi bir şubedeki ders seçilebildiğinden tüm dersler listelenir.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CourseOptions()
    {
        var result = await courseService.GetAllAsync();
        var options = result.Data?.Select(c => new
        {
            id = c.Id,
            name = $"{c.LanguageName} - {c.Level} / {c.BranchName} ({c.Day} {c.StartTime:hh\\:mm}) - {c.Fee:N2} ₺"
        });
        return Json(options);
    }

    /// <summary>
    /// Bir öğrenciyi bir derse kaydeder ve ödeme planını (taksitleri) oluşturur .
    /// </summary>
    /// <param name="dto">Kayıt ve ödeme verisi.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Kayıt", "Kayıt")]
    public async Task<IActionResult> Enroll(EnrollmentCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Lütfen tüm alanları doldurun." });
        }

        var result = await enrollmentService.EnrollAsync(dto);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>
    /// Bir öğrencinin tüm kayıtlarını ve taksit durumlarını JSON döndürür.
    /// "Ödenmemiş taksitler gösterilmeli" gereksinimini karşılar.
    /// </summary>
    /// <param name="studentId">Öğrenci kimliği.</param>
    [HttpGet]
    public async Task<IActionResult> StudentEnrollments(int studentId)
    {
        var result = await enrollmentService.GetByStudentAsync(studentId);
        return Json(new { success = result.Success, data = result.Data });
    }

    /// <summary>
    /// Belirli bir taksiti "ödendi" olarak işaretler .
    /// </summary>
    /// <param name="installmentId">Ödenen taksitin kimliği.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Audit("Ödeme", "Tahsilat")]
    public async Task<IActionResult> PayInstallment(int installmentId)
    {
        var result = await enrollmentService.PayInstallmentAsync(installmentId);
        return Json(new { success = result.Success, message = result.Message });
    }

    /// <summary>
    /// Ödenmiş bir taksit için PDF ödeme makbuzu üretir ve indirilebilir dosya olarak döndürür.
    /// </summary>
    /// <param name="installmentId">Makbuzu istenen taksitin kimliği.</param>
    [HttpGet]
    public async Task<IActionResult> Receipt(int installmentId)
    {
        // Makbuz verisi iş katmanından alınır (yalnızca ödenmiş taksitler için üretilir).
        var result = await enrollmentService.GetReceiptAsync(installmentId);
        if (!result.Success || result.Data is null)
        {
            return NotFound(result.Message);
        }

        // Veri, QuestPDF belgesine verilerek PDF baytları oluşturulur.
        var document = new ReceiptDocument(result.Data);
        var pdfBytes = document.GeneratePdf();

        // Tarayıcıda indirme için dosya olarak döndürülür.
        return File(pdfBytes, "application/pdf", $"{result.Data.ReceiptNo}.pdf");
    }
}
