using DilKursu.Entities.Common;

namespace DilKursu.Entities;

public class TeacherBranch : BaseEntity
{
    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public int BranchId { get; set; }

    public Branch Branch { get; set; } = null!;
}
