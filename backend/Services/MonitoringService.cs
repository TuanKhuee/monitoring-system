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
        
        // Mimic a modern Google Chrome browser on Windows exactly to bypass Vercel/Cloudflare bot protection
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "vi-VN,vi;q=0.9,en-US;q=0.8,en;q=0.7");
        _httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
        _httpClient.DefaultRequestHeaders.Add("Pragma", "no-cache");
        _httpClient.DefaultRequestHeaders.Add("sec-ch-ua", "\"Not A(Brand\";v=\"99\", \"Google Chrome\";v=\"121\", \"Chromium\";v=\"121\"");
        _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
        _httpClient.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
        _httpClient.DefaultRequestHeaders.Add("sec-fetch-dest", "document");
        _httpClient.DefaultRequestHeaders.Add("sec-fetch-mode", "navigate");
        _httpClient.DefaultRequestHeaders.Add("sec-fetch-site", "none");
        _httpClient.DefaultRequestHeaders.Add("sec-fetch-user", "?1");
        _httpClient.DefaultRequestHeaders.Add("upgrade-insecure-requests", "1");
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


                var servicesToCheck = activeServices.Where(s =>
                {
                    if (string.IsNullOrEmpty(s.Id)) return false;
                    if (!_lastCheckedAt.TryGetValue(s.Id, out var lastTime)) return true;
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

        var project = await projectRepository.GetByIdAsync(service.ProjectId);
        string projectName = project?.Name ?? "Unknown Project";

        var log = new MonitorLog
        {
            ServiceId = service.Id,
            ServiceName = service.ServiceName,
            ProjectName = projectName,
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
                var portStr = "";
                if ((service.Protocol.Equals("Http", StringComparison.OrdinalIgnoreCase) && service.Port != 80) ||
                    (service.Protocol.Equals("Https", StringComparison.OrdinalIgnoreCase) && service.Port != 443))
                {
                    portStr = $":{service.Port}";
                }
                var url = $"{service.Protocol.ToLower()}://{service.Ip}{portStr}{service.HealthEndpoint ?? ""}";
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
                    // Use the project fetched earlier
                    if (project != null)
                    {
                        var emailsToNotify = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                        if (!string.IsNullOrEmpty(project.OwnerId))
                        {
                            var owner = await userRepository.GetByIdAsync(project.OwnerId);
                            if (owner != null && !string.IsNullOrEmpty(owner.Email) && owner.IsEmailVerified)
                            {
                                emailsToNotify.Add(owner.Email);
                            }
                        }

                        if (project.NotifyEmails != null)
                        {
                            foreach (var email in project.NotifyEmails)
                            {
                                if (!string.IsNullOrWhiteSpace(email))
                                    emailsToNotify.Add(email.Trim());
                            }
                        }

                        var statusText = log.Status == 1 ? "ONLINE" : "OFFLINE";
                        
                        foreach (var email in emailsToNotify)
                        {
                            try 
                            {
                                await emailService.SendAlertEmailAsync(
                                    email,
                                    service.ServiceName,
                                    service.Ip,
                                    service.Port,
                                    statusText,
                                    log.ErrorMassage ?? "N/A"
                                );
                                _logger.LogInformation("Alert email sent to {Email} for service [{ServiceName}] status change to {Status}",
                                    email, service.ServiceName, statusText);
                            }
                            catch (Exception mailEx)
                            {
                                _logger.LogError(mailEx, "Failed to send alert email to {Email}", email);
                            }
                        }
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Failed to process alert emails for service {ServiceId}", service.Id);
                }
            }
        }
        
        // Update the last checked time
        _lastCheckedAt[service.Id] = DateTime.UtcNow;
    }

}
