using DilKursu.Business.Common;
using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.Identity;
using DilKursu.DataAccess.UnitOfWork;
using Microsoft.AspNetCore.Identity;

namespace DilKursu.Business.Services.Concrete;

public class UserService(UserManager<ApplicationUser> userManager, IUnitOfWork uow) : IUserService
{
    /// <summary>
    /// Tüm userları rol ve şube bilgisiyle birlikte döndürür.
    /// </summary>
    /// <returns>İşlem sonucu ve user listesi.</returns>
    public async Task<ServiceResult<IReadOnlyList<AppUserDto>>> GetAllAsync()
    {
        // Şube adlarını hızlı çözmek için tüm şubeleri bir sözlüğe al.
        var branches = await uow.Branches.GetAllAsync();
        var branchNames = branches.ToDictionary(b => b.Id, b => b.Name);

        var list = new List<AppUserDto>();
        foreach (var user in userManager.Users.ToList())
        {
            // Her kullanıcının rolü ayrı sorgulanır (kullanıcı sayısı azdır).
            var roles = await userManager.GetRolesAsync(user);
            list.Add(new AppUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? string.Empty,
                BranchId = user.BranchId,
                BranchName = user.BranchId.HasValue && branchNames.TryGetValue(user.BranchId.Value, out var name)
                    ? name
                    : AppUserClaimsPrincipalFactory.CentralLabel
            });
        }

        return ServiceResult<IReadOnlyList<AppUserDto>>.Ok(list);
    }

    /// <summary>
    /// Belirli bir userı düzenleme için (rol ve şube dahil) döndürür.
    /// </summary>
    /// <param name="id">User idsi.</param>
    /// <returns>İşlem sonucu ve user bilgisi.</returns>
    public async Task<ServiceResult<UserUpsertDto>> GetForEditAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return ServiceResult<UserUpsertDto>.Fail("Kullanıcı bulunamadı.");
        }

        var roles = await userManager.GetRolesAsync(user);
        return ServiceResult<UserUpsertDto>.Ok(new UserUpsertDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = roles.FirstOrDefault() ?? string.Empty,
            BranchId = user.BranchId
        });
    }

    /// <summary>
    /// Yeni bir user oluşturur, rolünü atar ve şubesini belirler.
    /// </summary>
    /// <param name="dto">User verileri.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> CreateAsync(UserUpsertDto dto)
    {
        var validation = await ValidateAsync(dto, isCreate: true);
        if (!validation.Success)
        {
            return validation;
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            EmailConfirmed = true,
            FullName = dto.FullName,
            // Yönetici merkezidir (şubesiz); kayıt elemanı seçilen şubeye bağlanır.
            BranchId = dto.Role == AppRoles.Admin ? null : dto.BranchId
        };

        var created = await userManager.CreateAsync(user, dto.Password!);
        if (!created.Succeeded)
        {
            return ServiceResult.Fail(FirstError(created));
        }

        await userManager.AddToRoleAsync(user, dto.Role);
        return ServiceResult.Ok("Kullanıcı başarıyla oluşturuldu.");
    }

    /// <summary>
    /// Mevcut bir userı günceller (bilgiler, rol, şube ve isteğe bağlı parola).
    /// </summary>
    /// <param name="dto">Güncellenecek user verileri.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> UpdateAsync(UserUpsertDto dto)
    {
        var user = await userManager.FindByIdAsync(dto.Id ?? string.Empty);
        if (user is null)
        {
            return ServiceResult.Fail("Güncellenecek kullanıcı bulunamadı.");
        }

        var validation = await ValidateAsync(dto, isCreate: false);
        if (!validation.Success)
        {
            return validation;
        }

        // Temel bilgiler güncellenir.
        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.BranchId = dto.Role == AppRoles.Admin ? null : dto.BranchId;

        var updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded)
        {
            return ServiceResult.Fail(FirstError(updated));
        }

        // Rol değişmişse eski roller kaldırılıp yeni rol atanır.
        var currentRoles = await userManager.GetRolesAsync(user);
        if (!currentRoles.Contains(dto.Role))
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            await userManager.AddToRoleAsync(user, dto.Role);
        }

        // Parola girildiyse sıfırlanır (boşsa mevcut parola korunur).
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await userManager.ResetPasswordAsync(user, token, dto.Password);
            if (!reset.Succeeded)
            {
                return ServiceResult.Fail(FirstError(reset));
            }
        }

        return ServiceResult.Ok("Kullanıcı başarıyla güncellendi.");
    }

    /// <summary>
    /// Bir userı siler. Sistemdeki son yönetici silinemez.
    /// </summary>
    /// <param name="id">Silinecek user idsi.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> DeleteAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return ServiceResult.Fail("Silinecek kullanıcı bulunamadı.");
        }

        // İş kuralı: sistemde en az bir yönetici kalmalıdır (son yönetici silinemez).
        if (await userManager.IsInRoleAsync(user, AppRoles.Admin))
        {
            var admins = await userManager.GetUsersInRoleAsync(AppRoles.Admin);
            if (admins.Count <= 1)
            {
                return ServiceResult.Fail("Sistemdeki son yönetici silinemez.");
            }
        }

        var deleted = await userManager.DeleteAsync(user);
        return deleted.Succeeded
            ? ServiceResult.Ok("Kullanıcı başarıyla silindi.")
            : ServiceResult.Fail(FirstError(deleted));
    }

    /// <summary>
    /// Kullanıcı oluşturma/güncelleme için ortak doğrulamalar (rol, parola, e-posta benzersizliği, şube).
    /// </summary>
    private async Task<ServiceResult> ValidateAsync(UserUpsertDto dto, bool isCreate)
    {
        // Rol, tanımlı roller arasında olmalı.
        if (!AppRoles.All.Contains(dto.Role))
        {
            return ServiceResult.Fail("Geçersiz rol.");
        }

        // Oluşturmada parola zorunludur.
        if (isCreate && string.IsNullOrWhiteSpace(dto.Password))
        {
            return ServiceResult.Fail("Yeni kullanıcı için parola zorunludur.");
        }

        // Kayıt elemanı için bir şube seçilmiş olmalı ve şube gerçekten var olmalı.
        if (dto.Role == AppRoles.Kayit)
        {
            if (!dto.BranchId.HasValue)
            {
                return ServiceResult.Fail("Kayıt elemanı için bir şube seçilmelidir.");
            }

            var branchExists = await uow.Branches.AnyAsync(b => b.Id == dto.BranchId.Value);
            if (!branchExists)
            {
                return ServiceResult.Fail("Seçilen şube bulunamadı.");
            }
        }

        // Aynı e-posta ile başka bir kullanıcı olmamalı.
        var existing = await userManager.FindByEmailAsync(dto.Email);
        if (existing is not null && existing.Id != dto.Id)
        {
            return ServiceResult.Fail("Bu e-posta ile bir kullanıcı zaten mevcut.");
        }

        return ServiceResult.Ok();
    }

    /// <summary>Identity sonucundaki ilk hata mesajını döndürür.</summary>
    private static string FirstError(IdentityResult result)
        => result.Errors.FirstOrDefault()?.Description ?? "İşlem başarısız oldu.";
}
