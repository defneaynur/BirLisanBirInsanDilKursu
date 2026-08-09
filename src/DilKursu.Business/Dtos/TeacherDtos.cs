using System.ComponentModel.DataAnnotations;

namespace DilKursu.Business.Dtos;

public class AvailabilityDto
{
    public DayOfWeek Day { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }
}

public class TeacherDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string HomePhone { get; set; } = string.Empty;

    public string MobilePhone { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public List<string> Languages { get; set; } = new();

    public List<string> Branches { get; set; } = new();
}

public class TeacherUpsertDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(20)]
    public string HomePhone { get; set; } = string.Empty;

    [StringLength(20)]
    public string MobilePhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "İşe başlama tarihi zorunludur.")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    public List<int> LanguageIds { get; set; } = new();

    public List<int> BranchIds { get; set; } = new();

    public List<AvailabilityDto> Availabilities { get; set; } = new();
}
