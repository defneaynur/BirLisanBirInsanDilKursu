using DilKursu.Entities.Common;
using DilKursu.Entities.Enums;

namespace DilKursu.Entities;

public class Enrollment : BaseEntity
{
    public int StudentId { get; set; }

    public Student Student { get; set; } = null!;

    public int CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public DateTime EnrollmentDate { get; set; } = DateTime.Now;

    public OdemeTuru PaymentType { get; set; }

    public decimal TotalAmount { get; set; }

    public ICollection<Installment> Installments { get; set; } = new List<Installment>();
}
