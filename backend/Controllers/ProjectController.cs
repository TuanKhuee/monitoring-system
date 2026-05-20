using System.Security.Claims;
using backend.DTOs.Project;
using backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var projects = await _projectService.GetAllProjectsAsync();
        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(string id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound(new { message = "Project not found" });
        }
        return Ok(project);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProjectRequest request)
    {
        var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(ownerId))
        {
            return Unauthorized(new { message = "User is unauthorized" });
        }

        var createdProject = await _projectService.CreateProjectAsync(request, ownerId);
        return CreatedAtAction("GetById", new { id = createdProject.Id }, createdProject);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(string id, [FromBody] UpdateProjectRequest request)
    {
        var success = await _projectService.UpdateProjectAsync(id, request);
        if (!success)
        {
            return NotFound(new { message = "Project not found to update" });
        }
        return Ok(new { message = "Project updated successfully" });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        var success = await _projectService.DeleteProjectAsync(id);
        if (!success)
        {
            return NotFound(new { message = "Project not found to delete" });
        }
        return Ok(new { message = "Project deleted successfully" });
    }
}
