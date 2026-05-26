using System.Security.Claims;
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
    private readonly IProjectService _projectService;

    public ServiceController(IServiceService serviceService, IProjectService projectService)
    {
        _serviceService = serviceService;
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var services = await _serviceService.GetAllServicesAsync();

        if (role != "Admin")
        {
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var userProjects = await _projectService.GetProjectsByOwnerIdAsync(userId);
            var projectIds = userProjects.Select(p => p.Id).ToHashSet();
            services = services.Where(s => projectIds.Contains(s.ProjectId));
        }

        return Ok(services);
    }

    [HttpGet("Project/{projectId}")]
    public async Task<IActionResult> GetByProjectAsync(string projectId)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (role != "Admin")
        {
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var project = await _projectService.GetByIdAsync(projectId);
            if (project == null) return NotFound();
            // Need to verify if the project actually belongs to the user
            var userProjects = await _projectService.GetProjectsByOwnerIdAsync(userId);
            if (!userProjects.Any(p => p.Id == projectId))
            {
                return StatusCode(403, new { message = "Bạn không có quyền truy cập vào Project này." });
            }
        }

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

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateServiceRequest request)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (role != "Admin")
        {
            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { message = "User is unauthorized" });
            var userProjects = await _projectService.GetProjectsByOwnerIdAsync(userId);
            if (!userProjects.Any(p => p.Id == request.ProjectId))
            {
                return StatusCode(403, new { message = "Bạn không có quyền thêm Service vào Project này." });
            }
        }

        var service = await _serviceService.CreateServiceAsync(request);
        return CreatedAtAction("GetById", new { id = service.Id }, service);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(string id, [FromBody] UpdateServiceRequest request)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (role != "Admin")
        {
            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { message = "User is unauthorized" });
            var existingService = await _serviceService.GetByIdAsync(id);
            if (existingService == null) return NotFound(new { message = "Service not found to update" });

            var userProjects = await _projectService.GetProjectsByOwnerIdAsync(userId);
            if (!userProjects.Any(p => p.Id == existingService.ProjectId))
            {
                return StatusCode(403, new { message = "Bạn không có quyền chỉnh sửa Service này." });
            }
        }

        var success = await _serviceService.UpdateServiceAsync(id, request);
        if (!success)
        {
            return NotFound(new { message = "Service not found to update" });
        }
        return Ok(new { message = "Service updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (role != "Admin")
        {
            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { message = "User is unauthorized" });
            var existingService = await _serviceService.GetByIdAsync(id);
            if (existingService == null) return NotFound(new { message = "Service not found to delete" });

            var userProjects = await _projectService.GetProjectsByOwnerIdAsync(userId);
            if (!userProjects.Any(p => p.Id == existingService.ProjectId))
            {
                return StatusCode(403, new { message = "Bạn không có quyền xóa Service này." });
            }
        }

        var success = await _serviceService.DeleteServiceAsync(id);
        if (!success)
        {
            return NotFound(new { message = "Service not found to delete" });
        }
        return Ok(new { message = "Service deleted successfully" });
    }
}
