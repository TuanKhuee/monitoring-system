using System.Security.Claims;
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
    private readonly IProjectRepository _projectRepository;

    public DashboardController(IServiceRepository serviceRepository, IMonitorLogRepository monitorLogRepository, IProjectRepository projectRepository)
    {
        _serviceRepository = serviceRepository;
        _monitorLogRepository = monitorLogRepository;
        _projectRepository = projectRepository;
    }

    [HttpGet("Stats")]
    public async Task<IActionResult> GetStatsAsync()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var services = await _serviceRepository.GetAllAsync();

        if (role != "Admin")
        {
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var userProjects = await _projectRepository.FilterByAsync(p => p.OwnerId == userId);
            var projectIds = userProjects.Select(p => p.Id).ToHashSet();
            services = services.Where(s => projectIds.Contains(s.ProjectId)).ToList();
        }

        var totalServices = services.Count();
        var activeServicesCount = services.Count(s => s.IsActive);
        var serviceIds = services.Select(s => s.Id).ToHashSet();

        // Lấy toàn bộ logs kiểm tra trong 24 giờ qua
        var since24h = DateTime.UtcNow.AddDays(-1);
        var recentLogs = await _monitorLogRepository.FilterByAsync(l => l.CheckedAt >= since24h);
        
        // Chỉ lấy log của các service thuộc quyền của user
        recentLogs = recentLogs.Where(l => serviceIds.Contains(l.ServiceId)).ToList();

        double uptimePercentage = 100.0;
        int onlineCount = 0;
        int offlineCount = 0;

        // Nhóm log mới nhất theo từng Service để tìm trạng thái hiện tại (ONLINE / OFFLINE)
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

        // Tính tỉ lệ Uptime của tất cả services
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
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var logs = await _monitorLogRepository.GetAllAsync();

        if (role != "Admin")
        {
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            var userProjects = await _projectRepository.FilterByAsync(p => p.OwnerId == userId);
            var projectIds = userProjects.Select(p => p.Id).ToHashSet();
            var services = await _serviceRepository.GetAllAsync();
            var serviceIds = services.Where(s => projectIds.Contains(s.ProjectId)).Select(s => s.Id).ToHashSet();
            logs = logs.Where(l => serviceIds.Contains(l.ServiceId)).ToList();
        }

        var sortedLogs = logs
            .OrderByDescending(l => l.CheckedAt)
            .Take(limit)
            .ToList();

        return Ok(sortedLogs);
    }
}
