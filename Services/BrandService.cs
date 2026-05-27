using CarDealershipAdoNet.DTOs;
using CarDealershipAdoNet.Repositories;

namespace CarDealershipAdoNet.Services;

public interface IBrandService
{
    Task<int> CreateAsync(CreateBrandRequest request);
    Task<IReadOnlyList<BrandResponse>> GetAllAsync();
    Task<BrandResponse?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, UpdateBrandRequest request);
    Task<bool> DeleteAsync(int id);
}

public sealed class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;

    public BrandService(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public Task<int> CreateAsync(CreateBrandRequest request)
    {
        return _brandRepository.CreateAsync(request);
    }

    public Task<IReadOnlyList<BrandResponse>> GetAllAsync()
    {
        return _brandRepository.GetAllAsync();
    }

    public Task<BrandResponse?> GetByIdAsync(int id)
    {
        return _brandRepository.GetByIdAsync(id);
    }

    public Task<bool> UpdateAsync(int id, UpdateBrandRequest request)
    {
        return _brandRepository.UpdateAsync(id, request);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _brandRepository.DeleteAsync(id);
    }
}
