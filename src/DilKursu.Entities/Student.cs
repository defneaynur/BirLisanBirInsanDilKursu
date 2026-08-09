using DilKursu.Entities.Common;

namespace DilKursu.Entities;

public class Student : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string HomePhone { get; set; } = string.Empty;

    public string MobilePhone { get; set; } = string.Empty;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
