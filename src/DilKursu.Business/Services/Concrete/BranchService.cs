using DilKursu.Business.Common;
using DilKursu.Business.Dtos;
using DilKursu.Business.Services.Abstract;
using DilKursu.DataAccess.UnitOfWork;
using DilKursu.Entities;
using Microsoft.EntityFrameworkCore;

namespace DilKursu.Business.Services.Concrete;

public class BranchService(IUnitOfWork uow) : IBranchService
{
    /// <summary>
    /// Tüm aktif şubeleri, derslik sayısı özetiyle birlikte döndürür.
    /// </summary>
    /// <returns></returns>
    public async Task<ServiceResult<IReadOnlyList<BranchDto>>> GetAllAsync()
    {
        // Şubeler, derslik sayısını hesaplayabilmek için Classrooms ilişkisiyle birlikte çekilir.
        var branches = await uow.Branches.GetAllAsync(b => b.Classrooms);

        // Her şube DTO'ya dönüştürülür; yalnızca aktif derslikler sayılır.
        var list = branches.Select(MapToDto).ToList();

        return ServiceResult<IReadOnlyList<BranchDto>>.Ok(list);
    }

    /// <summary>
    /// Id ile tek bir şubeyi döndürür.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ServiceResult<BranchDto>> GetByIdAsync(int id)
    {
        var branch = await uow.Branches.GetAsync(b => b.Id == id, b => b.Classrooms);
        if (branch is null)
        {
            return ServiceResult<BranchDto>.Fail("Şube bulunamadı.");
        }

        return ServiceResult<BranchDto>.Ok(MapToDto(branch));
    }

    /// <summary>
    ///  Yeni bir şube oluşturur. Aynı isimde aktif bir şube varsa hata döner.
    /// </summary>
    /// <param name="dto">Şube verileri.</param>
    /// <returns></returns>
    public async Task<ServiceResult> CreateAsync(BranchUpsertDto dto)
    {
        // Aynı isimde aktif bir şube olup olmadığı kontrol edilir (iş kuralı).
        var exists = await uow.Branches.AnyAsync(b => b.Name == dto.Name);
        if (exists)
        {
            return ServiceResult.Fail("Bu isimde bir şube zaten mevcut.");
        }

        // DTO'dan yeni entity oluşturulur.
        var branch = new Branch
        {
            Name = dto.Name,
            Address = dto.Address,
            PublicTransportInstructions = dto.PublicTransportInstructions,
            CarTransportInstructions = dto.CarTransportInstructions,
            SocialFacilities = dto.SocialFacilities
        };

        await uow.Branches.AddAsync(branch);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Şube başarıyla oluşturuldu.");
    }

    /// <summary>
    /// Mevcut bir şube günceller. Aynı isimde başka bir aktif şube varsa hata döner.
    /// </summary>
    /// <param name="dto">Güncellenecek şube verileri.</param>
    /// <returns></returns>
    public async Task<ServiceResult> UpdateAsync(BranchUpsertDto dto)
    {
        var branch = await uow.Branches.GetByIdAsync(dto.Id);
        if (branch is null)
        {
            return ServiceResult.Fail("Güncellenecek şube bulunamadı.");
        }

        // Aynı isimde, kendisi dışında başka bir şube var mı?
        var nameTaken = await uow.Branches.AnyAsync(b => b.Name == dto.Name && b.Id != dto.Id);
        if (nameTaken)
        {
            return ServiceResult.Fail("Bu isimde başka bir şube zaten mevcut.");
        }

        // Alanlar güncellenir.
        branch.Name = dto.Name;
        branch.Address = dto.Address;
        branch.PublicTransportInstructions = dto.PublicTransportInstructions;
        branch.CarTransportInstructions = dto.CarTransportInstructions;
        branch.SocialFacilities = dto.SocialFacilities;

        uow.Branches.Update(branch);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Şube başarıyla güncellendi.");
    }
        
    /// <summary>
    /// Şubeyi siler. Şubede aktif dersler varsa silme işlemi gerçekleştirilmez.
    /// </summary>
    /// <param name="id">Silinecek şube idsi.</param>
    /// <returns></returns>
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var branch = await uow.Branches.GetByIdAsync(id);
        if (branch is null)
        {
            return ServiceResult.Fail("Silinecek şube bulunamadı.");
        }

        // Şubede aktif ders varsa silmeye izin verilmez.
        var hasCourses = await uow.Courses.AnyAsync(c => c.BranchId == id);
        if (hasCourses)
        {
            return ServiceResult.Fail("Bu şubede açılmış dersler bulunduğu için şube silinemez.");
        }

        uow.Branches.Remove(branch);
        await uow.SaveChangesAsync();

        return ServiceResult.Ok("Şube başarıyla silindi.");
    }

    /// <summary>
    /// Bir Branch entity'sini BranchDto'ya dönüştürür.
    /// Dönüşüm mantığını tek noktada toplar (DRY).
    /// </summary>
    /// <param name="branch">Kaynak şube entity'si.</param>
    /// <returns>Eşlenmiş DTO.</returns>
    private static BranchDto MapToDto(Branch branch) => new()
    {
        Id = branch.Id,
        Name = branch.Name,
        Address = branch.Address,
        PublicTransportInstructions = branch.PublicTransportInstructions,
        CarTransportInstructions = branch.CarTransportInstructions,
        SocialFacilities = branch.SocialFacilities,
        ClassroomCount = branch.Classrooms?.Count(c => c.IsActive) ?? 0
    };
}
