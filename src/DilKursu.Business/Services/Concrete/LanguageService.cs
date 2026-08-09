using DilKursu.Business.Common;
using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.UnitOfWork;
using DilKursu.Entities;

namespace DilKursu.Business.Services.Concrete;

public class LanguageService(IUnitOfWork uow) : ILanguageService
{
    /// <summary>
    /// Tüm dilleri getirir.
    /// </summary>
    /// <returns>İşlem sonucu ve dil listesi.</returns>
    public async Task<ServiceResult<IReadOnlyList<LanguageDto>>> GetAllAsync()
    {
        var languages = await uow.Languages.GetAllAsync();
        var list = languages.Select(MapToDto).ToList();
        return ServiceResult<IReadOnlyList<LanguageDto>>.Ok(list);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip dili getirir.
    /// </summary>
    /// <param name="id">Getirilecek dilin ID'si.</param>
    /// <returns>İşlem sonucu ve dil bilgisi.</returns>
    public async Task<ServiceResult<LanguageDto>> GetByIdAsync(int id)
    {
        var language = await uow.Languages.GetByIdAsync(id);
        if (language is null)
        {
            return ServiceResult<LanguageDto>.Fail("Dil bulunamadı.");
        }

        return ServiceResult<LanguageDto>.Ok(MapToDto(language));
    }

    /// <summary>
    /// Yeni bir dil oluşturur. Aynı isimde bir dil varsa hata döner.
    /// </summary>
    /// <param name="dto">Dil form verisi.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> CreateAsync(LanguageUpsertDto dto)
    {
        // Aynı dil adının tekrar eklenmesi engellenir (benzersizlik iş kuralı).
        var exists = await uow.Languages.AnyAsync(l => l.Name == dto.Name);
        if (exists)
        {
            return ServiceResult.Fail("Bu dil zaten tanımlı.");
        }

        var language = new Language { Name = dto.Name };

        await uow.Languages.AddAsync(language);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Dil başarıyla eklendi.");
    }

    /// <summary>
    /// Belirtilen ID'ye sahip dili günceller.
    /// </summary>
    /// <param name="dto">Güncellenecek dil bilgilerini içeren DTO.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> UpdateAsync(LanguageUpsertDto dto)
    {
        var language = await uow.Languages.GetByIdAsync(dto.Id);
        if (language is null)
        {
            return ServiceResult.Fail("Güncellenecek dil bulunamadı.");
        }

        var nameTaken = await uow.Languages.AnyAsync(l => l.Name == dto.Name && l.Id != dto.Id);
        if (nameTaken)
        {
            return ServiceResult.Fail("Bu isimde başka bir dil zaten mevcut.");
        }

        language.Name = dto.Name;

        uow.Languages.Update(language);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Dil başarıyla güncellendi.");
    }

    /// <summary>
    /// Belirtilen ID'ye sahip dili siler.
    /// </summary>
    /// <param name="id">Silinecek dilin ID'si.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var language = await uow.Languages.GetByIdAsync(id);
        if (language is null)
        {
            return ServiceResult.Fail("Silinecek dil bulunamadı.");
        }

        // Bu dilde açılmış aktif ders varsa silinemez.
        var hasCourses = await uow.Courses.AnyAsync(c => c.LanguageId == id);
        if (hasCourses)
        {
            return ServiceResult.Fail("Bu dilde açılmış dersler bulunduğu için dil silinemez.");
        }

        uow.Languages.Remove(language);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Dil başarıyla silindi.");
    }

    /// <summary>
    /// Language entity'sini LanguageDto'ya dönüştürür.
    /// </summary>
    /// <param name="language">Kaynak dil entity'si.</param>
    /// <returns>Eşlenmiş DTO.</returns>
    private static LanguageDto MapToDto(Language language) => new()
    {
        Id = language.Id,
        Name = language.Name
    };
}
