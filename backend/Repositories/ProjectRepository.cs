using backend.Model;
using backend.Repositories.Interfaces;
using backend.Settings;
using Microsoft.Extensions.Options;

namespace backend.Repositories;

public class ProjectRepository : MongoRepository<Project>, IProjectRepository
{
    public ProjectRepository(IOptions<MongoDbSettings> settings) : base(settings)
    {
    }
}
