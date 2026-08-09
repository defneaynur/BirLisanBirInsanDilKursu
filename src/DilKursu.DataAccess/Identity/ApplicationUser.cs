using Microsoft.AspNetCore.Identity;

namespace DilKursu.DataAccess.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public int? BranchId { get; set; }
}
