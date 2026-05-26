using backend.DTOs.Service;
using backend.Model;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services;

public class ServiceService : IServiceService
{
    private readonly IServiceRepository _serviceRepository;

    public ServiceService(IServiceRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }
    public async Task<IEnumerable<ServiceResponse>> GetAllServicesAsync()
    {
        var services = await _serviceRepository.GetAllAsync();
        return services.Select(MapToResponse).ToList();
    }

    public async Task<ServiceResponse?> GetByIdAsync(string id)
    {
         var service = await _serviceRepository.GetByIdAsync(id);
        if (service == null) return null;
        return MapToResponse(service);
    }

    public async Task<IEnumerable<ServiceResponse>> GetServicesByProjectIdAsync(string projectId)
    {
        var services = await _serviceRepository.FilterByAsync(s=>s.ProjectId == projectId);
        return services.Select(MapToResponse).ToList();
    }
    public async Task<ServiceResponse> CreateServiceAsync(CreateServiceRequest request)
    {
        var service = new Service
        {
            ProjectId = request.ProjectId,
            ServiceName = request.ServiceName,
            Ip = request.Ip,
            Port = request.Port,
            Protocol = request.Protocol,
            HealthEndpoint = request.HealthEndpoint,
            IntervalSeconds = request.IntervalSeconds,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        await _serviceRepository.InsertOneAsync(service);
        return MapToResponse(service);
    }

     public async Task<bool> DeleteServiceAsync(string id)
    {
        var existingService = await _serviceRepository.GetByIdAsync(id);
        if (existingService == null) return false;
        await _serviceRepository.DeleteByIdAsync(id);
        return true;
    }

    public async Task<bool> UpdateServiceAsync(string id, UpdateServiceRequest request)
    {
        var existingService = await _serviceRepository.GetByIdAsync(id);
        if (existingService == null) return false;
        existingService.ServiceName = request.ServiceName;
        existingService.Ip = request.Ip;
        existingService.Port = request.Port;
        existingService.Protocol = request.Protocol;
        existingService.HealthEndpoint = request.HealthEndpoint;
        existingService.IntervalSeconds = request.IntervalSeconds;
        existingService.IsActive = request.IsActive;
        await _serviceRepository.ReplaceOneAsync(id, existingService);
        return true;
    }

    private ServiceResponse MapToResponse(Service service)
    {
        return new ServiceResponse
        {
            Id = service.Id ?? string.Empty,
            ProjectId = service.ProjectId,
            ServiceName = service.ServiceName,
            Ip = service.Ip,
            Port = service.Port,
            Protocol = service.Protocol,
            HealthEndpoint = service.HealthEndpoint,
            IntervalSeconds = service.IntervalSeconds,
            IsActive = service.IsActive,
            CreatedAt = service.CreatedAt
        };
    }
}