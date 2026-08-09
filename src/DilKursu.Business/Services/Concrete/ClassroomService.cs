using DilKursu.Business.Common;
using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.UnitOfWork;
using DilKursu.Entities;

namespace DilKursu.Business.Services.Concrete;

public class ClassroomService(IUnitOfWork uow) : IClassroomService
{
    /// <summary>
    /// Tüm derslikleri döndürür.   
    /// </summary>
    /// <returns>Tüm dersliklerin DTO listesi.</returns>
    public async Task<ServiceResult<IReadOnlyList<ClassroomDto>>> GetAllAsync()
    {
        var classrooms = await uow.Classrooms.GetAllAsync(c => c.Branch);
        var list = classrooms.Select(MapToDto).ToList();
        return ServiceResult<IReadOnlyList<ClassroomDto>>.Ok(list);
    }

    /// <summary>
    /// Derslikleri şube idsi ile filtreleyerek döndürür.
    /// </summary>
    /// <param name="branchId">Şube idsi.</param>
    /// <returns></returns>
    public async Task<ServiceResult<IReadOnlyList<ClassroomDto>>> GetByBranchAsync(int branchId)
    {
        // Yalnızca ilgili şubenin derslikleri süzülür.
        var classrooms = await uow.Classrooms.FindAsync(c => c.BranchId == branchId, c => c.Branch);
        var list = classrooms.Select(MapToDto).ToList();
        return ServiceResult<IReadOnlyList<ClassroomDto>>.Ok(list);
    }

    /// <summary>
    /// Id ile tek bir dersliği döndürür.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ServiceResult<ClassroomDto>> GetByIdAsync(int id)
    {
        var classroom = await uow.Classrooms.GetAsync(c => c.Id == id, c => c.Branch);
        if (classroom is null)
        {
            return ServiceResult<ClassroomDto>.Fail("Derslik bulunamadı.");
        }

        return ServiceResult<ClassroomDto>.Ok(MapToDto(classroom));
    }

    /// <summary>
    /// Derslik oluşturur. Aynı şubede aynı isimde bir derslik varsa hata döner.
    /// </summary>
    /// <param name="dto">Oluşturulacak derslik verileri.</param>
    /// <returns></returns>
    public async Task<ServiceResult> CreateAsync(ClassroomUpsertDto dto)
    {
        // Seçilen şubenin varlığı doğrulanır.
        var branch = await uow.Branches.GetByIdAsync(dto.BranchId);
        if (branch is null)
        {
            return ServiceResult.Fail("Seçilen şube bulunamadı.");
        }

        // Aynı şubede aynı isimde derslik olmamalı.
        var exists = await uow.Classrooms.AnyAsync(c => c.BranchId == dto.BranchId && c.Name == dto.Name);
        if (exists)
        {
            return ServiceResult.Fail("Bu şubede aynı isimde bir derslik zaten mevcut.");
        }

        var classroom = new Classroom
        {
            Name = dto.Name,
            Capacity = dto.Capacity,
            BranchId = dto.BranchId
        };

        await uow.Classrooms.AddAsync(classroom);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Derslik başarıyla oluşturuldu.");
    }

    /// <summary>
    /// Güncellenmiş derslik verilerini alır ve mevcut dersliği günceller.
    /// </summary>
    /// <param name="dto">Güncellenecek derslik verileri.</param>
    /// <returns></returns>
    public async Task<ServiceResult> UpdateAsync(ClassroomUpsertDto dto)
    {
        var classroom = await uow.Classrooms.GetByIdAsync(dto.Id);
        if (classroom is null)
        {
            return ServiceResult.Fail("Güncellenecek derslik bulunamadı.");
        }

        var branch = await uow.Branches.GetByIdAsync(dto.BranchId);
        if (branch is null)
        {
            return ServiceResult.Fail("Seçilen şube bulunamadı.");
        }

        classroom.Name = dto.Name;
        classroom.Capacity = dto.Capacity;
        classroom.BranchId = dto.BranchId;

        uow.Classrooms.Update(classroom);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Derslik başarıyla güncellendi.");
    }

    /// <summary>
    /// Dersliği siler. Derslikte aktif dersler varsa silme işlemi gerçekleştirilmez.
    /// </summary>
    /// <param name="id">Silinecek derslik idsi.</param>
    /// <returns></returns>
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var classroom = await uow.Classrooms.GetByIdAsync(id);
        if (classroom is null)
        {
            return ServiceResult.Fail("Silinecek derslik bulunamadı.");
        }

        // Derslikte açılmış aktif ders varsa silinemez.
        var hasCourses = await uow.Courses.AnyAsync(c => c.ClassroomId == id);
        if (hasCourses)
        {
            return ServiceResult.Fail("Bu derslikte açılmış dersler bulunduğu için derslik silinemez.");
        }

        uow.Classrooms.Remove(classroom);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Derslik başarıyla silindi.");
    }

    /// <summary>
    /// Classroom entity'sini ClassroomDto'ya dönüştürür.
    /// </summary>
    /// <param name="classroom">Kaynak derslik entity'si.</param>
    /// <returns>Eşlenmiş DTO.</returns>
    private static ClassroomDto MapToDto(Classroom classroom) => new()
    {
        Id = classroom.Id,
        Name = classroom.Name,
        Capacity = classroom.Capacity,
        BranchId = classroom.BranchId,
        BranchName = classroom.Branch?.Name ?? string.Empty
    };
}
