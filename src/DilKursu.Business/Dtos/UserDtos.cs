using System.ComponentModel.DataAnnotations;

namespace DilKursu.Business.Dtos;

public class AppUserDto
{
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int? BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;
}

public class UserUpsertDto
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>Oluşturmada zorunlu; güncellemede boş bırakılırsa parola değişmez.</summary>
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Parola en az 6 karakter olmalıdır.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Rol seçilmelidir.")]
    public string Role { get; set; } = string.Empty;

    /// <summary>Kullanıcının bağlı olduğu şube; boş ise merkez (tüm şubeler) kabul edilir.</summary>
    public int? BranchId { get; set; }
}
