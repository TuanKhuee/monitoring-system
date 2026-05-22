using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using backend.Model;
using backend.Repositories;
using backend.Repositories.Interfaces;
using backend.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace backend.Services;

public class MonitoringService: BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MonitoringService> _logger;
    private readonly HttpClient _httpClient;

    // Tracks the last known status of each service to avoid spamming the database
    private readonly ConcurrentDictionary<string, int> _lastServiceStatus = new();

    // Tracks the last time each service was checked (for per-service interval)
    private readonly ConcurrentDictionary<string, DateTime> _lastCheckedAt = new();

    public MonitoringService(IServiceProvider serviceProvider, ILogger<MonitoringService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _httpClient = new HttpClient{ Timeout = TimeSpan.FromSeconds(10)};
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Monitoring Service is starting.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var serviceRepository = scope.ServiceProvider.GetRequiredService<IServiceRepository>();
                var monitorLogRepository = scope.ServiceProvider.GetRequiredService<IMonitorLogRepository>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var projectRepository = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var activeServices = await serviceRepository.FilterByAsync(s => s.IsActive);
                var now = DateTime.UtcNow;

                // Only check services that are due:
                //   - Never checked before  → check immediately (first-time)
                //   - Elapsed time >= service's own IntervalSeconds
                var servicesToCheck = activeServices.Where(s =>
                {
                    if (string.IsNullOrEmpty(s.Id)) return false;
                    if (!_lastCheckedAt.TryGetValue(s.Id, out var lastTime)) return true; // never checked → immediate
                    return (now - lastTime).TotalSeconds >= s.IntervalSeconds;
                }).ToList();

                if (servicesToCheck.Count > 0)
                {
                    var tasks = servicesToCheck.Select(service =>
                        CheckAndLogServiceAsync(service, monitorLogRepository, emailService, projectRepository, userRepository));
                    await Task.WhenAll(tasks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Monitoring Service execution loop.");
            }

            // Short polling interval: re-scan every 2 seconds to catch newly added services quickly
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
        _logger.LogInformation("Background Monitoring Service is stopping.");
    }

    private async Task CheckAndLogServiceAsync(
        Service service, 
        IMonitorLogRepository monitorLogRepository,
        IEmailService emailService,
        IProjectRepository projectRepository,
        IUserRepository userRepository)
    {
        if (string.IsNullOrEmpty(service.Id)) return;

        var log = new MonitorLog
        {
            ServiceId = service.Id,
            CheckedAt = DateTime.UtcNow,  
        };
        
        var stopwatch = Stopwatch.StartNew();
        try
        {
            if(service.Protocol.Equals("Ping", StringComparison.OrdinalIgnoreCase))
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(service.Ip, 5000);
                stopwatch.Stop();

                log.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                log.Status = reply.Status == IPStatus.Success ? 1 : 0;
                log.StatusCode = reply.Status == IPStatus.Success ? 200 : (int)reply.Status;
                log.ErrorMassage = reply.Status == IPStatus.Success ? "Ping Successfull" : $"Ping failed: {reply.Status}";

            }
            else if(service.Protocol.Equals("Tcp", StringComparison.OrdinalIgnoreCase))
            {
                using var tcpClient = new TcpClient();
                var connectTask = tcpClient.ConnectAsync(service.Ip, service.Port);
                var delayTask = Task.Delay(5000);

                var completedTask = await Task.WhenAny(connectTask, delayTask);
                stopwatch.Stop();

                log.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                if(completedTask == connectTask && tcpClient.Connected)
                {
                    log.Status = 1;
                    log.StatusCode = 200;
                    log.ErrorMassage = "TCP Connect Successful";
                }
                else
                {
                    log.Status = 0;
                    log.StatusCode = 503;
                    log.ErrorMassage = "TCP Connect Timeout or Connection Refused";
                }
            }
            else
            {
                var url = $"{service.Protocol.ToLower()}://{service.Ip}:{service.Port}{service.HealthEndpoint ?? ""}";
                var response = await _httpClient.GetAsync(url);
                stopwatch.Stop();

                log.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                log.StatusCode = (int)response.StatusCode;
                log.Status = response.IsSuccessStatusCode ? 1 : 0;
                log.ErrorMassage = response.IsSuccessStatusCode ? "HTTP Get Successfull" : $"HTTP failed with status: {response.StatusCode}";
            }
        }
        catch(Exception ex)
        {
            stopwatch.Stop();
            log.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            log.Status = 0;
            log.StatusCode = 500;
            log.ErrorMassage = $"Check failed: {ex.Message}";
        }

        // Determine if we should save the log to DB
        bool shouldSaveLog = false;
        bool isStatusChange = false;
        if (!_lastServiceStatus.TryGetValue(service.Id, out int lastStatus))
        {
            // First time check, save log
            shouldSaveLog = true;
            _lastServiceStatus[service.Id] = log.Status;
        }
        else if (lastStatus != log.Status)
        {
            // Status changed, save log and send email
            shouldSaveLog = true;
            isStatusChange = true;
            _lastServiceStatus[service.Id] = log.Status;
        }

        if (shouldSaveLog)
        {
            try
            {
                await monitorLogRepository.InsertOneAsync(log);
                _logger.LogInformation("Status Changed. Service [{ServiceName}] ({Ip}:{Port}) Checked. Status: {Status} | Time: {Time}ms",
                    service.ServiceName, service.Ip, service.Port, log.Status == 1 ? "ONLINE" : "OFFLINE", log.ResponseTimeMs);
            }
            catch(Exception dbEx)
            {
                _logger.LogError(dbEx, "Failed to save monitor log for service {ServiceId}", service.Id);
            }

            // Send alert email when status changes (not on first check)
            if (isStatusChange)
            {
                try
                {
                    // Find project owner's email
                    var project = await projectRepository.GetByIdAsync(service.ProjectId);
                    if (project != null && !string.IsNullOrEmpty(project.OwnerId))
                    {
                        var owner = await userRepository.GetByIdAsync(project.OwnerId);
                        if (owner != null && !string.IsNullOrEmpty(owner.Email) && owner.IsEmailVerified)
                        {
                            var statusText = log.Status == 1 ? "ONLINE" : "OFFLINE";
                            await emailService.SendAlertEmailAsync(
                                owner.Email,
                                service.ServiceName,
                                service.Ip,
                                service.Port,
                                statusText,
                                log.ErrorMassage ?? "N/A"
                            );
                            _logger.LogInformation("Alert email sent to {Email} for service [{ServiceName}] status change to {Status}",
                                owner.Email, service.ServiceName, statusText);
                        }
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to send alert email for service {ServiceId}", service.Id);
                }
            }
        }
        
        // Update the last checked time
        _lastCheckedAt[service.Id] = DateTime.UtcNow;
    }

}
