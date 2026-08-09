using System.ComponentModel.DataAnnotations;

namespace DilKursu.Business.Dtos;

public class LanguageDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

public class LanguageUpsertDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Dil adı zorunludur.")]
    [StringLength(80, ErrorMessage = "Dil adı en fazla 80 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;
}
