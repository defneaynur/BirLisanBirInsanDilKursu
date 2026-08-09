using System.ComponentModel.DataAnnotations;
using DilKursu.Entities.Enums;

namespace DilKursu.Business.Dtos;

public class EnrollmentCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Öğrenci seçilmelidir.")]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Ders seçilmelidir.")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Ödeme türü seçilmelidir.")]
    public OdemeTuru PaymentType { get; set; }

    [Range(1, 24, ErrorMessage = "Taksit sayısı 1 ile 24 arasında olmalıdır.")]
    public int InstallmentCount { get; set; } = 1;
}

public class InstallmentDto
{
    public int Id { get; set; }

    public int SequenceNo { get; set; }

    public DateTime DueDate { get; set; }

    public decimal Amount { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaidDate { get; set; }
}

public class EnrollmentDetailDto
{
    public int Id { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string CourseInfo { get; set; } = string.Empty;

    public OdemeTuru PaymentType { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public List<InstallmentDto> Installments { get; set; } = new();
}
