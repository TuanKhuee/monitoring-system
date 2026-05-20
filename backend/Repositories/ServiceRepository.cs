using backend.Model;
using backend.Repositories.Interfaces;
using backend.Settings;
using Microsoft.Extensions.Options;

namespace backend.Repositories;

public class ServiceRepository : MongoRepository<Service>, IServiceRepository
{
    public ServiceRepository(IOptions<MongoDbSettings> settings) : base(settings)
    {
    }
}
