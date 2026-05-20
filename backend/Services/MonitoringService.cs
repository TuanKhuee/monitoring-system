using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using backend.Model;
using backend.Repositories;
using backend.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace backend.Services;

public class MonitoringService: BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MonitoringService> _logger;
    private readonly HttpClient _httpClient;

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
                
                var activeServices = await serviceRepository.FilterByAsync(s => s.IsActive);
                
                var tasks = activeServices.Select(service => CheckAndLogServiceAsync(service, monitorLogRepository));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in Monitoring Service execution loop.");
            }
            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
        _logger.LogInformation("Background Monitoring Service is stopping.");
    }
    private async Task CheckAndLogServiceAsync(Service service, IMonitorLogRepository monitorLogRepository)
    {
        var log = new MonitorLog
        {
            ServiceId = service.Id ?? string.Empty,
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

        try
        {
            await monitorLogRepository.InsertOneAsync(log);
            _logger.LogInformation("Service [{ServiceName}] ({Ip}: {Port}) Checked.Status: {Status} | Time: {Time}ms",
            service.ServiceName,
            service.Ip, service.Port, log.Status == 1 ? "ONLINE" : "OFFLINE", log.ResponseTimeMs);
        }
        catch(Exception dbEx)
        {
            _logger.LogError(dbEx, "Failed to save monitor log for service {ServiceId}",service.Id);
        }
    }

}
