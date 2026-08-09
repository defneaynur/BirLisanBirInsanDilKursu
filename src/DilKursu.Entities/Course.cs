using DilKursu.Entities.Common;
using DilKursu.Entities.Enums;

namespace DilKursu.Entities;

public class Course : BaseEntity
{
    public int LanguageId { get; set; }

    public Language Language { get; set; } = null!;

    public KurSeviyesi Level { get; set; }

    public int BranchId { get; set; }
  
    public Branch Branch { get; set; } = null!;

    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public int ClassroomId { get; set; }

    public Classroom Classroom { get; set; } = null!;


    public DayOfWeek Day { get; set; }


    public TimeSpan StartTime { get; set; }


    public TimeSpan EndTime { get; set; }

    public DateTime StartDate { get; set; }

    public int Quota { get; set; }

    public decimal Fee { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
