using System.ComponentModel.DataAnnotations;
using DilKursu.Entities.Enums;

namespace DilKursu.Business.Dtos;

public class CourseDto
{
    public int Id { get; set; }

    public string LanguageName { get; set; } = string.Empty;

    public KurSeviyesi Level { get; set; }

    public string BranchName { get; set; } = string.Empty;

    public string TeacherName { get; set; } = string.Empty;

    public string ClassroomName { get; set; } = string.Empty;

    public DayOfWeek Day { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public decimal Fee { get; set; }

    public int Quota { get; set; }

    public int EnrolledCount { get; set; }
}

public class CourseCriteriaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Dil seçilmelidir.")]
    public int LanguageId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Şube seçilmelidir.")]
    public int BranchId { get; set; }

    [Required(ErrorMessage = "Gün seçilmelidir.")]
    public DayOfWeek Day { get; set; }

    [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "Bitiş saati zorunludur.")]
    public TimeSpan EndTime { get; set; }
}

public class CourseSuggestionDto
{
    public List<TeacherOptionDto> AvailableTeachers { get; set; } = new();

    public List<ClassroomOptionDto> AvailableClassrooms { get; set; } = new();
}

public class TeacherOptionDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;
}

public class ClassroomOptionDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }
}

public class CourseCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Dil seçilmelidir.")]
    public int LanguageId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Şube seçilmelidir.")]
    public int BranchId { get; set; }

    [Required(ErrorMessage = "Kur seçilmelidir.")]
    public KurSeviyesi Level { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Öğretmen seçilmelidir.")]
    public int TeacherId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Derslik seçilmelidir.")]
    public int ClassroomId { get; set; }

    [Required(ErrorMessage = "Gün seçilmelidir.")]
    public DayOfWeek Day { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required(ErrorMessage = "Başlangıç tarihi zorunludur.")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Range(1, 500, ErrorMessage = "Kontenjan 1 ile 500 arasında olmalıdır.")]
    public int Quota { get; set; }

    [Range(0, 1000000, ErrorMessage = "Ücret geçerli bir tutar olmalıdır.")]
    public decimal Fee { get; set; }
}
