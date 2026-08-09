using DilKursu.Entities.Common;

namespace DilKursu.Entities;

public class Teacher : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string HomePhone { get; set; } = string.Empty;

    public string MobilePhone { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public ICollection<TeacherLanguage> TeacherLanguages { get; set; } = new List<TeacherLanguage>();

    public ICollection<TeacherBranch> TeacherBranches { get; set; } = new List<TeacherBranch>();

    public ICollection<TeacherAvailability> Availabilities { get; set; } = new List<TeacherAvailability>();

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
