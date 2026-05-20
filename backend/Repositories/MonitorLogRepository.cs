using backend.Model;
using backend.Repositories.Interfaces;
using backend.Settings;
using Microsoft.Extensions.Options;

namespace backend.Repositories;

public class MonitorLogRepository : MongoRepository<MonitorLog>, IMonitorLogRepository
{
    public MonitorLogRepository(IOptions<MongoDbSettings> settings) : base(settings)
    {
    }
}
