using DilKursu.Business.Common;
using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.UnitOfWork;
using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;

namespace DilKursu.Business.Services.Concrete;

public class TeacherService(IUnitOfWork uow) : ITeacherService
{
    /// <summary>
    /// Tüm aktif öğretmenleri dil ve şube özetleriyle döndürür.
    /// </summary>
    /// <returns>İşlem sonucu ve öğretmen listesi.</returns>
    public async Task<ServiceResult<IReadOnlyList<TeacherDto>>> GetAllAsync()
    {
        // Öğretmenler; dil ve şube adlarını gösterebilmek için ilişkili grafikleriyle çekilir.
        var teachers = await uow.Teachers.Query()
            .Include(t => t.TeacherLanguages).ThenInclude(tl => tl.Language)
            .Include(t => t.TeacherBranches).ThenInclude(tb => tb.Branch)
            .AsNoTracking()
            .ToListAsync();

        var list = teachers.Select(t => new TeacherDto
        {
            Id = t.Id,
            FullName = t.FullName,
            HomePhone = t.HomePhone,
            MobilePhone = t.MobilePhone,
            StartDate = t.StartDate,
            Languages = t.TeacherLanguages.Where(tl => tl.IsActive && tl.Language != null)
                                          .Select(tl => tl.Language.Name).ToList(),
            Branches = t.TeacherBranches.Where(tb => tb.IsActive && tb.Branch != null)
                                        .Select(tb => tb.Branch.Name).ToList()
        }).ToList();

        return ServiceResult<IReadOnlyList<TeacherDto>>.Ok(list);
    }

    /// <summary>
    /// Idye göre öğretmeni, düzenleme formuna uygun ilişkileriyle (dil, şube, müsaitlik) döndürür.
    /// </summary>
    /// <param name="id">Öğretmen idsi.</param>
    /// <returns>İşlem sonucu ve öğretmen bilgisi.</returns>
    public async Task<ServiceResult<TeacherUpsertDto>> GetForEditAsync(int id)
    {
        var teacher = await uow.Teachers.Query()
            .Include(t => t.TeacherLanguages)
            .Include(t => t.TeacherBranches)
            .Include(t => t.Availabilities)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher is null)
        {
            return ServiceResult<TeacherUpsertDto>.Fail("Öğretmen bulunamadı.");
        }

        // Entity, form doldurmaya uygun DTO'ya dönüştürülür.
        var dto = new TeacherUpsertDto
        {
            Id = teacher.Id,
            FullName = teacher.FullName,
            HomePhone = teacher.HomePhone,
            MobilePhone = teacher.MobilePhone,
            StartDate = teacher.StartDate,
            LanguageIds = teacher.TeacherLanguages.Where(tl => tl.IsActive).Select(tl => tl.LanguageId).ToList(),
            BranchIds = teacher.TeacherBranches.Where(tb => tb.IsActive).Select(tb => tb.BranchId).ToList(),
            Availabilities = teacher.Availabilities.Where(a => a.IsActive).Select(a => new AvailabilityDto
            {
                Day = a.Day,
                StartTime = a.StartTime,
                EndTime = a.EndTime
            }).ToList()
        };

        return ServiceResult<TeacherUpsertDto>.Ok(dto);
    }

    /// <summary>
    /// Diller, şubeler ve müsaitlikler ile birlikte yeni bir öğretmen oluşturur.
    /// </summary>
    /// <param name="dto">Öğretmen form verisi.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> CreateAsync(TeacherUpsertDto dto)
    {
        // İş kuralı: öğretmenin en az bir dil ve bir şube ile ilişkilendirilmesi zorunludur.
        var validation = ValidateRelations(dto);
        if (!validation.Success)
        {
            return validation;
        }

        var teacher = new Teacher
        {
            FullName = dto.FullName,
            HomePhone = dto.HomePhone,
            MobilePhone = dto.MobilePhone,
            StartDate = dto.StartDate
        };

        // Seçilen diller, şubeler ve müsaitlikler bağlantı varlıkları olarak eklenir.
        ApplyRelations(teacher, dto);

        await uow.Teachers.AddAsync(teacher);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Öğretmen başarıyla kaydedildi.");
    }

    /// <summary>
    /// Mevcut bir öğretmeni, dil/şube/müsaitlik ilişkileriyle birlikte günceller.
    /// </summary>
    /// <param name="dto">Güncellenecek öğretmen verisi.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> UpdateAsync(TeacherUpsertDto dto)
    {
        var validation = ValidateRelations(dto);
        if (!validation.Success)
        {
            return validation;
        }

        // Güncellemede ilişkileri yeniden kurabilmek için mevcut grafiği izlenir (tracked) olarak çekiyoruz.
        var teacher = await uow.Teachers.Query()
            .Include(t => t.TeacherLanguages)
            .Include(t => t.TeacherBranches)
            .Include(t => t.Availabilities)
            .FirstOrDefaultAsync(t => t.Id == dto.Id);

        if (teacher is null)
        {
            return ServiceResult.Fail("Güncellenecek öğretmen bulunamadı.");
        }

        teacher.FullName = dto.FullName;
        teacher.HomePhone = dto.HomePhone;
        teacher.MobilePhone = dto.MobilePhone;
        teacher.StartDate = dto.StartDate;

        // Eski ilişkiler temizlenip yeni seçimlere göre yeniden kurulur (senkronizasyon).
        teacher.TeacherLanguages.Clear();
        teacher.TeacherBranches.Clear();
        teacher.Availabilities.Clear();
        ApplyRelations(teacher, dto);

        uow.Teachers.Update(teacher);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Öğretmen başarıyla güncellendi.");
    }

    /// <summary>
    /// Bir öğretmeni siler. Öğretmen bir derse atanmışsa silme işlemi gerçekleştirilmez.
    /// </summary>
    /// <param name="id">Silinecek öğretmen idsi.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var teacher = await uow.Teachers.GetByIdAsync(id);
        if (teacher is null)
        {
            return ServiceResult.Fail("Silinecek öğretmen bulunamadı.");
        }

        // Öğretmen bir derse atalıysa silinemez.
        var hasCourses = await uow.Courses.AnyAsync(c => c.TeacherId == id);
        if (hasCourses)
        {
            return ServiceResult.Fail("Bu öğretmen bir derse atanmış olduğu için silinemez.");
        }

        uow.Teachers.Remove(teacher);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Öğretmen başarıyla silindi.");
    }

    /// <summary>
    /// Öğretmen için en az bir dil ve bir şube seçilip seçilmediğini doğrular (ortak iş kuralı, DRY).
    /// </summary>
    /// <param name="dto">Doğrulanacak öğretmen verisi.</param>
    /// <returns>Doğrulama sonucu.</returns>
    private static ServiceResult ValidateRelations(TeacherUpsertDto dto)
    {
        if (dto.LanguageIds is null || dto.LanguageIds.Count == 0)
        {
            return ServiceResult.Fail("En az bir dil seçilmelidir.");
        }

        if (dto.BranchIds is null || dto.BranchIds.Count == 0)
        {
            return ServiceResult.Fail("En az bir şube seçilmelidir.");
        }

        // Her müsaitlik penceresinde başlangıç saati bitişten önce olmalıdır.
        if (dto.Availabilities.Any(a => a.StartTime >= a.EndTime))
        {
            return ServiceResult.Fail("Müsaitlik saatlerinde başlangıç, bitişten önce olmalıdır.");
        }

        return ServiceResult.Ok();
    }

    /// <summary>
    /// DTO'daki seçilmiş dil, şube ve müsaitlikleri öğretmen entity'sine bağlantı varlıkları olarak ekler.
    /// Oluşturma ve güncelleme akışlarında ortak kullanılır (DRY).
    /// </summary>
    /// <param name="teacher">İlişkilerin ekleneceği öğretmen entity'si.</param>
    /// <param name="dto">Kaynak veri.</param>
    private static void ApplyRelations(Teacher teacher, TeacherUpsertDto dto)
    {
        foreach (var languageId in dto.LanguageIds.Distinct())
        {
            teacher.TeacherLanguages.Add(new TeacherLanguage { LanguageId = languageId });
        }

        foreach (var branchId in dto.BranchIds.Distinct())
        {
            teacher.TeacherBranches.Add(new TeacherBranch { BranchId = branchId });
        }

        foreach (var availability in dto.Availabilities)
        {
            teacher.Availabilities.Add(new TeacherAvailability
            {
                Day = availability.Day,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime
            });
        }
    }
}
