using DilKursu.Entities.Common;

namespace DilKursu.Entities;

public class Classroom : BaseEntity
{

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public int BranchId { get; set; }

    public Branch Branch { get; set; } = null!;

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
