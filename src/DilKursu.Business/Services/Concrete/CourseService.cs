using DilKursu.Business.Common;
using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.UnitOfWork;
using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;

namespace DilKursu.Business.Services.Concrete;

public class CourseService(IUnitOfWork uow) : ICourseService
{
    /// <summary>
    /// Tüm dersleri döndürür.
    /// </summary>
    /// <returns>Tüm derslerin DTO listesi.</returns>
    public async Task<ServiceResult<IReadOnlyList<CourseDto>>> GetAllAsync()
    {
        // Dersler; listede dil, şube, öğretmen ve derslik adlarını gösterebilmek için ilişkileriyle çekilir.
        var courses = await uow.Courses.Query()
            .Include(c => c.Language)
            .Include(c => c.Branch)
            .Include(c => c.Teacher)
            .Include(c => c.Classroom)
            .Include(c => c.Enrollments)
            .AsNoTracking()
            .ToListAsync();

        var list = courses.Select(c => new CourseDto
        {
            Id = c.Id,
            LanguageName = c.Language.Name,
            Level = c.Level,
            BranchName = c.Branch.Name,
            TeacherName = c.Teacher.FullName,
            ClassroomName = c.Classroom.Name,
            Day = c.Day,
            StartTime = c.StartTime,
            EndTime = c.EndTime,
            Fee = c.Fee,
            Quota = c.Quota,
            EnrolledCount = c.Enrollments.Count(e => e.IsActive)
        }).ToList();

        return ServiceResult<IReadOnlyList<CourseDto>>.Ok(list);
    }

    /// <summary>
    /// Verilen kriterlere göre uygun öğretmenleri ve derslikleri önerir.
    /// </summary>
    /// <param name="criteria">Kriterler.</param>
    /// <returns>Uygun öğretmenler ve dersliklerin DTO'su.</returns>
    public async Task<ServiceResult<CourseSuggestionDto>> GetSuggestionsAsync(CourseCriteriaDto criteria)
    {
        // Girdi doğrulaması: saat aralığı mantıklı olmalı.
        if (criteria.StartTime >= criteria.EndTime)
        {
            return ServiceResult<CourseSuggestionDto>.Fail("Başlangıç saati bitiş saatinden önce olmalıdır.");
        }

        var suggestion = new CourseSuggestionDto
        {
            AvailableTeachers = await FindAvailableTeachersAsync(criteria),
            AvailableClassrooms = await FindAvailableClassroomsAsync(criteria),

            // Bağlam sayıları: "hiç yok" ile "o saatte dolu/müsait değil" durumlarını ayırt etmek için.
            BranchClassroomCount = await uow.Classrooms.Query()
                .CountAsync(c => c.BranchId == criteria.BranchId),
            BranchLanguageTeacherCount = await uow.Teachers.Query()
                .CountAsync(t => t.TeacherLanguages.Any(tl => tl.IsActive && tl.LanguageId == criteria.LanguageId)
                              && t.TeacherBranches.Any(tb => tb.IsActive && tb.BranchId == criteria.BranchId))
        };

        return ServiceResult<CourseSuggestionDto>.Ok(suggestion);
    }

    /// <summary>
    /// Yeni bir ders oluşturur. Ders oluşturulmadan önce, seçilen öğretmenin ve dersliğin hâlâ uygun olup olmadığı tekrar doğrulanır.
    /// </summary>
    /// <param name="dto">Oluşturulacak ders verileri.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> CreateAsync(CourseCreateDto dto)
    {
        // Saat aralığı doğrulaması.
        if (dto.StartTime >= dto.EndTime)
        {
            return ServiceResult.Fail("Başlangıç saati bitiş saatinden önce olmalıdır.");
        }

        // Seçilen öğretmenin, kaydetme anında hâlâ uygun olduğunu tekrar doğrula (yarış durumu koruması).
        var criteria = new CourseCriteriaDto
        {
            LanguageId = dto.LanguageId,
            BranchId = dto.BranchId,
            Day = dto.Day,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime
        };

        var eligibleTeachers = await FindAvailableTeachersAsync(criteria);
        if (eligibleTeachers.All(t => t.Id != dto.TeacherId))
        {
            return ServiceResult.Fail("Seçilen öğretmen bu ders için artık uygun değil (dil, şube, müsaitlik veya çakışma).");
        }

        // Seçilen dersliğin hâlâ boş olduğunu tekrar doğrula.
        var eligibleClassrooms = await FindAvailableClassroomsAsync(criteria);
        if (eligibleClassrooms.All(r => r.Id != dto.ClassroomId))
        {
            return ServiceResult.Fail("Seçilen derslik bu saatte artık boş değil.");
        }

        var course = new Course
        {
            LanguageId = dto.LanguageId,
            BranchId = dto.BranchId,
            Level = dto.Level,
            TeacherId = dto.TeacherId,
            ClassroomId = dto.ClassroomId,
            Day = dto.Day,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            StartDate = dto.StartDate,
            Quota = dto.Quota,
            Fee = dto.Fee
        };

        await uow.Courses.AddAsync(course);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Ders başarıyla açıldı.");
    }

    /// <summary>
    /// Verilen ID'ye sahip dersi siler. Silme işlemi, dersin kayıtlı öğrenci olup olmadığını kontrol eder; eğer varsa silme işlemi engellenir.
    /// </summary>
    /// <param name="id">Silinecek dersin ID'si.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var course = await uow.Courses.GetByIdAsync(id);
        if (course is null)
        {
            return ServiceResult.Fail("Silinecek ders bulunamadı.");
        }

        // Derse kayıtlı öğrenci varsa silinemez.
        var hasEnrollments = await uow.Enrollments.AnyAsync(e => e.CourseId == id);
        if (hasEnrollments)
        {
            return ServiceResult.Fail("Bu derse kayıtlı öğrenciler bulunduğu için ders silinemez.");
        }

        uow.Courses.Remove(course);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Ders başarıyla silindi.");
    }

    /// <summary>
    /// Verilen kriterlere uyan öğretmenleri bulur. Bir öğretmenin uygun sayılması için:
    /// (1) istenen dili öğretebilmesi, (2) istenen şubede ders verebilmesi,
    /// (3) istenen gün ve saat aralığını kapsayan bir müsaitlik penceresine sahip olması,
    /// (4) aynı gün/saatte başka bir derse atanmamış (çakışmayan) olması gerekir.
    /// </summary>
    /// <param name="criteria">Ders açma kriterleri.</param>
    /// <returns>Uygun öğretmen seçeneklerinin listesi.</returns>
    private async Task<List<TeacherOptionDto>> FindAvailableTeachersAsync(CourseCriteriaDto criteria)
    {
        // (1) ve (2): dil ve şube filtresini veritabanı seviyesinde uygula, ilişkileri de yükle.
        var candidates = await uow.Teachers.Query()
            .Include(t => t.TeacherLanguages)
            .Include(t => t.TeacherBranches)
            .Include(t => t.Availabilities)
            .Include(t => t.Courses)
            .Where(t => t.TeacherLanguages.Any(tl => tl.IsActive && tl.LanguageId == criteria.LanguageId)
                     && t.TeacherBranches.Any(tb => tb.IsActive && tb.BranchId == criteria.BranchId))
            .AsNoTracking()
            .ToListAsync();

        var result = new List<TeacherOptionDto>();

        foreach (var teacher in candidates)
        {
            // (3): o gün için ders saatini tamamen kapsayan bir müsaitlik penceresi var mı?
            var isAvailable = teacher.Availabilities.Any(a =>
                a.IsActive &&
                a.Day == criteria.Day &&
                ScheduleHelper.Covers(a.StartTime, a.EndTime, criteria.StartTime, criteria.EndTime));

            if (!isAvailable)
            {
                continue;
            }

            // (4): aynı gün ve çakışan saatte başka bir derse atanmış mı?
            var hasConflict = teacher.Courses.Any(c =>
                c.IsActive &&
                c.Day == criteria.Day &&
                ScheduleHelper.Overlaps(c.StartTime, c.EndTime, criteria.StartTime, criteria.EndTime));

            if (hasConflict)
            {
                continue;
            }

            result.Add(new TeacherOptionDto { Id = teacher.Id, FullName = teacher.FullName });
        }

        return result;
    }

    /// <summary>
    /// Verilen kriterlere göre boş derslikleri bulur. Bir dersliğin boş sayılması için:
    /// (1) istenen şubeye ait olması, (2) aynı gün/saatte başka bir derse tahsis edilmemiş olması gerekir.
    /// </summary>
    /// <param name="criteria">Ders açma kriterleri.</param>
    /// <returns>Boş derslik seçeneklerinin listesi.</returns>
    private async Task<List<ClassroomOptionDto>> FindAvailableClassroomsAsync(CourseCriteriaDto criteria)
    {
        // (1): şubeye ait derslikleri, üzerlerindeki dersler ile birlikte çek.
        var classrooms = await uow.Classrooms.Query()
            .Include(c => c.Courses)
            .Where(c => c.BranchId == criteria.BranchId)
            .AsNoTracking()
            .ToListAsync();

        var result = new List<ClassroomOptionDto>();

        foreach (var classroom in classrooms)
        {
            // (2): aynı gün ve çakışan saatte bu derslikte başka ders var mı?
            var isOccupied = classroom.Courses.Any(c =>
                c.IsActive &&
                c.Day == criteria.Day &&
                ScheduleHelper.Overlaps(c.StartTime, c.EndTime, criteria.StartTime, criteria.EndTime));

            if (isOccupied)
            {
                continue;
            }

            result.Add(new ClassroomOptionDto
            {
                Id = classroom.Id,
                Name = classroom.Name,
                Capacity = classroom.Capacity
            });
        }

        return result;
    }
}
