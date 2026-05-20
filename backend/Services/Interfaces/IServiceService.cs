using backend.DTOs.Service;

namespace backend.Services.Interfaces;

public interface IServiceService
{
    Task<IEnumerable<ServiceResponse>> GetAllServicesAsync();
    Task<IEnumerable<ServiceResponse>> GetServicesByProjectIdAsync(string projectId);
    Task<ServiceResponse?> GetByIdAsync(string id);
    Task<ServiceResponse> CreateServiceAsync(CreateServiceRequest request);
    Task<bool> UpdateServiceAsync(string id, UpdateServiceRequest request);
    Task<bool> DeleteServiceAsync(string id);
}
