using DilKursu.Business.Common;
using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.UnitOfWork;
using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;

namespace DilKursu.Business.Services.Concrete;

public class StudentService(IUnitOfWork uow) : IStudentService
{
    /// <summary>
    /// Tüm aktif öğrencileri kayıt sayısı özetiyle döndürür.
    /// </summary>
    /// <returns>İşlem sonucu ve öğrenci listesi.</returns>
    public async Task<ServiceResult<IReadOnlyList<StudentDto>>> GetAllAsync()
    {
        // Kayıt sayısını gösterebilmek için öğrenciler Enrollments ilişkisiyle çekilir.
        var students = await uow.Students.Query()
            .Include(s => s.Enrollments)
            .AsNoTracking()
            .ToListAsync();

        var list = students.Select(s => new StudentDto
        {
            Id = s.Id,
            FullName = s.FullName,
            HomePhone = s.HomePhone,
            MobilePhone = s.MobilePhone,
            EnrollmentCount = s.Enrollments.Count(e => e.IsActive)
        }).ToList();

        return ServiceResult<IReadOnlyList<StudentDto>>.Ok(list);
    }

    /// <summary>
    /// Idye göre tek bir öğrenciyi döndürür.
    /// </summary>
    /// <param name="id">Öğrenci idsi.</param>
    /// <returns>İşlem sonucu ve öğrenci bilgisi.</returns>
    public async Task<ServiceResult<StudentDto>> GetByIdAsync(int id)
    {
        var student = await uow.Students.GetByIdAsync(id);
        if (student is null)
        {
            return ServiceResult<StudentDto>.Fail("Öğrenci bulunamadı.");
        }

        return ServiceResult<StudentDto>.Ok(new StudentDto
        {
            Id = student.Id,
            FullName = student.FullName,
            HomePhone = student.HomePhone,
            MobilePhone = student.MobilePhone
        });
    }

    /// <summary>
    /// Yeni bir öğrenci oluşturur.
    /// </summary>
    /// <param name="dto">Öğrenci form verisi.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> CreateAsync(StudentUpsertDto dto)
    {
        var student = new Student
        {
            FullName = dto.FullName,
            HomePhone = dto.HomePhone ?? string.Empty,
            MobilePhone = dto.MobilePhone
        };

        await uow.Students.AddAsync(student);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Öğrenci başarıyla kaydedildi.");
    }

    /// <summary>
    /// Mevcut bir öğrenciyi günceller.
    /// </summary>
    /// <param name="dto">Güncellenecek öğrenci verisi.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> UpdateAsync(StudentUpsertDto dto)
    {
        var student = await uow.Students.GetByIdAsync(dto.Id);
        if (student is null)
        {
            return ServiceResult.Fail("Güncellenecek öğrenci bulunamadı.");
        }

        student.FullName = dto.FullName;
        student.HomePhone = dto.HomePhone ?? string.Empty;
        student.MobilePhone = dto.MobilePhone;

        uow.Students.Update(student);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Öğrenci başarıyla güncellendi.");
    }

    /// <summary>
    /// Bir öğrenciyi siler. Öğrencinin aktif ders kaydı varsa silme işlemi gerçekleştirilmez.
    /// </summary>
    /// <param name="id">Silinecek öğrenci idsi.</param>
    /// <returns>İşlem sonucu.</returns>
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var student = await uow.Students.GetByIdAsync(id);
        if (student is null)
        {
            return ServiceResult.Fail("Silinecek öğrenci bulunamadı.");
        }

        // Öğrencinin aktif kaydı varsa silinemez.
        var hasEnrollments = await uow.Enrollments.AnyAsync(e => e.StudentId == id);
        if (hasEnrollments)
        {
            return ServiceResult.Fail("Bu öğrencinin ders kayıtları bulunduğu için silinemez.");
        }

        uow.Students.Remove(student);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Öğrenci başarıyla silindi.");
    }
}
