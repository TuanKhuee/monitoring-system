using backend.DTOs.Project;
using backend.Model;
using Microsoft.AspNetCore.Mvc;

namespace backend.Services.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<ProjectResponse>> GetAllProjectsAsync();
    Task<IEnumerable<ProjectResponse>> GetProjectsByOwnerIdAsync(string ownerId);
    Task<ProjectResponse?> GetByIdAsync(string id);
    Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest  request, string ownerId);
    Task<bool> DeleteProjectAsync(string id);
    Task<bool> UpdateProjectAsync(string id, UpdateProjectRequest request);
}