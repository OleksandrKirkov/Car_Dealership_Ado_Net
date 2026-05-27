using CarDealershipAdoNet.DTOs;
using CarDealershipAdoNet.Repositories;

namespace CarDealershipAdoNet.Services;

public interface ICarService
{
    Task<int> CreateAsync(CreateCarRequest request);
    Task<IReadOnlyList<CarViewResponse>> GetAllAsync();
    Task<CarViewResponse?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, UpdateCarRequest request);
    Task<bool> DeleteAsync(int id);
}

public sealed class CarService : ICarService
{
    private readonly ICarRepository _carRepository;

    public CarService(ICarRepository carRepository)
    {
        _carRepository = carRepository;
    }

    public Task<int> CreateAsync(CreateCarRequest request)
    {
        return _carRepository.CreateAsync(request);
    }

    public Task<IReadOnlyList<CarViewResponse>> GetAllAsync()
    {
        return _carRepository.GetAllAsync();
    }

    public Task<CarViewResponse?> GetByIdAsync(int id)
    {
        return _carRepository.GetByIdAsync(id);
    }

    public Task<bool> UpdateAsync(int id, UpdateCarRequest request)
    {
        return _carRepository.UpdateAsync(id, request);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _carRepository.DeleteAsync(id);
    }
}
