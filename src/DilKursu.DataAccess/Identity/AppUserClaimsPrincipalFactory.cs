using System.Security.Claims;
using DilKursu.DataAccess.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DilKursu.DataAccess.Identity;

public class AppUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options,
    AppDbContext context)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
{
    /// <summary>Şubesi olmayan (merkezi) kullanıcılar için gösterilecek etiket.</summary>
    public const string CentralLabel = "Merkez (Tüm Şubeler)";

    /// <summary>Şube bilgisini taşıyan claim'in adı.</summary>
    public const string BranchClaimType = "Branch";

    /// <summary>
    /// Varsayılan claim'lere ek olarak kullanıcının şube bilgisini "Branch" claim'i olarak ekler.
    /// </summary>
    /// <param name="user">Giriş yapan kullanıcı.</param>
    /// <returns>Şube claim'i eklenmiş kimlik.</returns>
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Kullanıcı bir şubeye bağlıysa şube adı, değilse merkez etiketi kullanılır.
        var branchName = CentralLabel;
        if (user.BranchId.HasValue)
        {
            var branch = await context.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == user.BranchId.Value);
            if (branch is not null)
            {
                branchName = branch.Name;
            }
        }

        identity.AddClaim(new Claim(BranchClaimType, branchName));
        return identity;
    }
}
