using System.ComponentModel.DataAnnotations;

namespace DilKursu.Business.Dtos;

public class StudentDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string HomePhone { get; set; } = string.Empty;

    public string MobilePhone { get; set; } = string.Empty;

    public int EnrollmentCount { get; set; }
}

public class StudentUpsertDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(20)]
    public string HomePhone { get; set; } = string.Empty;

    [StringLength(20)]
    public string MobilePhone { get; set; } = string.Empty;
}
