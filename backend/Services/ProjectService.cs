using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs.Project;
using backend.Model;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;

namespace backend.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IServiceRepository _serviceRepository;

        public ProjectService(IProjectRepository projectRepository, IServiceRepository serviceRepository)
        {
            _projectRepository = projectRepository;
            _serviceRepository = serviceRepository;
        }

        public async Task<IEnumerable<ProjectResponse>> GetAllProjectsAsync()
        {
            var projects = await _projectRepository.GetAllAsync();
            return projects.Select(MapToResponse).ToList();
        }

        public async Task<IEnumerable<ProjectResponse>> GetProjectsByOwnerIdAsync(string ownerId)
        {
            var projects = await _projectRepository.FilterByAsync(p => p.OwnerId == ownerId);
            return projects.Select(MapToResponse).ToList();
        }

        public async Task<ProjectResponse?> GetByIdAsync(string id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null) return null;
            return MapToResponse(project);
        }

        public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, string ownerId)
        {
            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
                Status = request.Status,
                ProjectCode = request.ProjectCode,
                ProjectUrl = request.ProjectUrl,
                RepositoryUrl = request.RepositoryUrl,
                NotifyEmails = request.NotifyEmails ?? new List<string>(),
                OwnerId = ownerId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Members = new List<string>()
            };

            await _projectRepository.InsertOneAsync(project);
            return MapToResponse(project);
        }

        public async Task<bool> UpdateProjectAsync(string id, UpdateProjectRequest request)
        {
            var existingProject = await _projectRepository.GetByIdAsync(id);
            if (existingProject == null) return false;

            existingProject.Name = request.Name;
            existingProject.Description = request.Description;
            existingProject.Status = request.Status;
            existingProject.ProjectCode = request.ProjectCode;
            existingProject.ProjectUrl = request.ProjectUrl;
            existingProject.RepositoryUrl = request.RepositoryUrl;
            existingProject.NotifyEmails = request.NotifyEmails ?? new List<string>();
            existingProject.UpdatedAt = DateTime.UtcNow;

            await _projectRepository.ReplaceOneAsync(id, existingProject);
            return true;
        }

        public async Task<bool> DeleteProjectAsync(string id)
        {
            var existingProject = await _projectRepository.GetByIdAsync(id);
            if (existingProject == null) return false;

            var services = await _serviceRepository.FilterByAsync(s => s.ProjectId == id);
            foreach (var svc in services)
            {
                if (!string.IsNullOrEmpty(svc.Id))
                {
                    await _serviceRepository.DeleteByIdAsync(svc.Id);
                }
            }

            await _projectRepository.DeleteByIdAsync(id);
            return true;
        }

        private ProjectResponse MapToResponse(Project project)
        {
            return new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                ProjectCode = project.ProjectCode,
                ProjectUrl = project.ProjectUrl,
                RepositoryUrl = project.RepositoryUrl,
                NotifyEmails = project.NotifyEmails ?? new List<string>(),
                CreatedDate = project.CreatedAt,
                UpdatedDate = project.UpdatedAt
            };
        }
    }
}