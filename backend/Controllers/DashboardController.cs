using backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IMonitorLogRepository _monitorLogRepository;

    public DashboardController(IServiceRepository serviceRepository, IMonitorLogRepository monitorLogRepository)
    {
        _serviceRepository = serviceRepository;
        _monitorLogRepository = monitorLogRepository;
    }

    [HttpGet("Stats")]
    public async Task<IActionResult> GetStatsAsync()
    {
        var services = await _serviceRepository.GetAllAsync();
        var totalServices = services.Count();
        var activeServicesCount = services.Count(s => s.IsActive);

        
        var since24h = DateTime.UtcNow.AddDays(-1);
        var recentLogs = await _monitorLogRepository.FilterByAsync(l => l.CheckedAt >= since24h);

        double uptimePercentage = 100.0;
        int onlineCount = 0;
        int offlineCount = 0;

        
        var latestLogsByService = recentLogs
            .GroupBy(l => l.ServiceId)
            .Select(g => g.OrderByDescending(l => l.CheckedAt).FirstOrDefault())
            .ToList();

        foreach (var log in latestLogsByService)
        {
            if (log != null)
            {
                if (log.Status == 1) onlineCount++;
                else offlineCount++;
            }
        }

        
        if (recentLogs.Any())
        {
            var successChecks = recentLogs.Count(l => l.Status == 1);
            var totalChecks = recentLogs.Count();
            uptimePercentage = Math.Round((double)successChecks / totalChecks * 100, 2);
        }

        return Ok(new
        {
            TotalServices = totalServices,
            ActiveServices = activeServicesCount,
            OnlineServices = onlineCount,
            OfflineServices = offlineCount,
            OverallUptime24h = uptimePercentage,
            ServerTime = DateTime.UtcNow
        });
    }

    [HttpGet("RecentLogs")]
    public async Task<IActionResult> GetRecentLogsAsync([FromQuery] int limit = 20)
    {
        var logs = await _monitorLogRepository.GetAllAsync();
        var sortedLogs = logs
            .OrderByDescending(l => l.CheckedAt)
            .Take(limit)
            .ToList();

        return Ok(sortedLogs);
    }
}
