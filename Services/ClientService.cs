using CarDealershipAdoNet.DTOs;
using CarDealershipAdoNet.Repositories;

namespace CarDealershipAdoNet.Services;

public interface IClientService
{
    Task<int> CreateAsync(CreateClientRequest request);
    Task<IReadOnlyList<ClientResponse>> GetAllAsync();
    Task<ClientResponse?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, UpdateClientRequest request);
    Task<bool> DeleteAsync(int id);
}

public sealed class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;

    public ClientService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }

    public Task<int> CreateAsync(CreateClientRequest request)
    {
        return _clientRepository.CreateAsync(request);
    }

    public Task<IReadOnlyList<ClientResponse>> GetAllAsync()
    {
        return _clientRepository.GetAllAsync();
    }

    public Task<ClientResponse?> GetByIdAsync(int id)
    {
        return _clientRepository.GetByIdAsync(id);
    }

    public Task<bool> UpdateAsync(int id, UpdateClientRequest request)
    {
        return _clientRepository.UpdateAsync(id, request);
    }

    public Task<bool> DeleteAsync(int id)
    {
        return _clientRepository.DeleteAsync(id);
    }
}
