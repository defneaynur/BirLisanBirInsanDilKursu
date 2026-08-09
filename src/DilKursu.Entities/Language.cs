using DilKursu.Entities.Common;

namespace DilKursu.Entities;

public class Language : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<TeacherLanguage> TeacherLanguages { get; set; } = new List<TeacherLanguage>();

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
