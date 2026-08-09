using DilKursu.Entities.Common;

namespace DilKursu.Entities;

public class TeacherLanguage : BaseEntity
{
    public int TeacherId { get; set; }

    public Teacher Teacher { get; set; } = null!;

    public int LanguageId { get; set; }

    public Language Language { get; set; } = null!;
}
