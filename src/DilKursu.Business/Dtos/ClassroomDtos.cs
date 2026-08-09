using System.ComponentModel.DataAnnotations;

namespace DilKursu.Business.Dtos;

public class ClassroomDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public int BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;
}

public class ClassroomUpsertDto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Derslik adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Derslik adı en fazla 100 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Range(1, 500, ErrorMessage = "Kapasite 1 ile 500 arasında olmalıdır.")]
    public int Capacity { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Şube seçilmelidir.")]
    public int BranchId { get; set; }
}
