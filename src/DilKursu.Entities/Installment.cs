using DilKursu.Entities.Common;

namespace DilKursu.Entities;

public class Installment : BaseEntity
{
    public int EnrollmentId { get; set; }

    public Enrollment Enrollment { get; set; } = null!;

    public int SequenceNo { get; set; }

    public DateTime DueDate { get; set; }

    public decimal Amount { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaidDate { get; set; }
}
