using backend.DTOs.Service;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ServiceController : ControllerBase
{
    private readonly IServiceService _serviceService;

    public ServiceController(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var services = await _serviceService.GetAllServicesAsync();
        return Ok(services);
    }

    [HttpGet("Project/{projectId}")]
    public async Task<IActionResult> GetByProjectAsync(string projectId)
    {
        var services = await _serviceService.GetServicesByProjectIdAsync(projectId);
        return Ok(services);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(string id)
    {
        var service = await _serviceService.GetByIdAsync(id);
        if (service == null)
        {
            return NotFound(new { message = "Service not found" });
        }
        return Ok(service);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateServiceRequest request)
    {
        var service = await _serviceService.CreateServiceAsync(request);
        return CreatedAtAction("GetById", new { id = service.Id }, service);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(string id, [FromBody] UpdateServiceRequest request)
    {
        var success = await _serviceService.UpdateServiceAsync(id, request);
        if (!success)
        {
            return NotFound(new { message = "Service not found to update" });
        }
        return Ok(new { message = "Service updated successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        var success = await _serviceService.DeleteServiceAsync(id);
        if (!success)
        {
            return NotFound(new { message = "Service not found to delete" });
        }
        return Ok(new { message = "Service deleted successfully" });
    }
}
