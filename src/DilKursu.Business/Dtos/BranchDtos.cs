using System.ComponentModel.DataAnnotations;

namespace DilKursu.Business.Dtos;

public class BranchDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PublicTransportInstructions { get; set; } = string.Empty;

    public string CarTransportInstructions { get; set; } = string.Empty;

    public string SocialFacilities { get; set; } = string.Empty;

    public int ClassroomCount { get; set; }
}

public class BranchUpsertDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Şube adı zorunludur.")]
    [StringLength(150, ErrorMessage = "Şube adı en fazla 150 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adres zorunludur.")]
    [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Toplu taşıma talimatları zorunludur.")]
    [StringLength(1000, ErrorMessage = "Toplu taşıma talimatları en fazla 1000 karakter olabilir.")]
    public string PublicTransportInstructions { get; set; } = string.Empty;

    [Required(ErrorMessage = "Özel otomobil talimatları zorunludur.")]
    [StringLength(1000, ErrorMessage = "Özel otomobil talimatları en fazla 1000 karakter olabilir.")]
    public string CarTransportInstructions { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sosyal olanaklar zorunludur.")]
    [StringLength(1000, ErrorMessage = "Sosyal olanaklar en fazla 1000 karakter olabilir.")]
    public string SocialFacilities { get; set; } = string.Empty;
}
