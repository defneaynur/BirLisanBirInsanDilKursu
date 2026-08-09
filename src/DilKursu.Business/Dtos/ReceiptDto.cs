using DilKursu.Entities.Enums;

namespace DilKursu.Business.Dtos;

public class ReceiptDto
{
    public string ReceiptNo { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string StudentPhone { get; set; } = string.Empty;

    public string CourseInfo { get; set; } = string.Empty;

    public OdemeTuru PaymentType { get; set; }

    public int InstallmentNo { get; set; }

    public decimal InstallmentAmount { get; set; }

    public DateTime? PaidDate { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RemainingAmount { get; set; }
}
