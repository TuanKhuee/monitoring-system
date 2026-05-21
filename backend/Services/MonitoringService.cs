using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using backend.Model;
using backend.Repositories;
using backend.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using backend.Services.Interfaces;

namespace backend.Services;

public class MonitoringService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MonitoringService> _logger;
    private readonly HttpClient _httpClient;

    public MonitoringService(IServiceProvider serviceProvider, ILogger<MonitoringService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
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
                // var monitorLogRepository = scope.ServiceProvider.GetRequiredService<IMonitorLogRepository>();

                var activeServices = await serviceRepository.FilterByAsync(s => s.IsActive);

                var tasks = activeServices.Select(service => CheckAndLogServiceAsync(service, scope.ServiceProvider));
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
    private async Task CheckAndLogServiceAsync(Service service, IServiceProvider serviceProvider)
    {
        var monitorLogRepository = serviceProvider.GetRequiredService<IMonitorLogRepository>();
        var projectRepository = serviceProvider.GetRequiredService<IProjectRepository>();
        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        var emailService = serviceProvider.GetRequiredService<IEmailService>();
        var log = new MonitorLog
        {
            ServiceId = service.Id ?? string.Empty,
            CheckedAt = DateTime.UtcNow,
        };

        var stopwatch = Stopwatch.StartNew();
        try
        {
            if (service.Protocol.Equals("Ping", StringComparison.OrdinalIgnoreCase))
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(service.Ip, 5000);
                stopwatch.Stop();

                log.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                log.Status = reply.Status == IPStatus.Success ? 1 : 0;
                log.StatusCode = reply.Status == IPStatus.Success ? 200 : (int)reply.Status;
                log.ErrorMassage = reply.Status == IPStatus.Success ? "Ping Successfull" : $"Ping failed: {reply.Status}";

            }
            else if (service.Protocol.Equals("Tcp", StringComparison.OrdinalIgnoreCase))
            {
                using var tcpClient = new TcpClient();
                var connectTask = tcpClient.ConnectAsync(service.Ip, service.Port);
                var delayTask = Task.Delay(5000);

                var completedTask = await Task.WhenAny(connectTask, delayTask);
                stopwatch.Stop();

                log.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
                if (completedTask == connectTask && tcpClient.Connected)
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
        catch (Exception ex)
        {
            stopwatch.Stop();
            log.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            log.Status = 0;
            log.StatusCode = 500;
            log.ErrorMassage = $"Check failed: {ex.Message}";
        }

        try
        {
            var historyLogs = await monitorLogRepository.FilterByAsync(l => l.ServiceId == service.Id);
            var lastLog = historyLogs.OrderByDescending(l => l.CheckedAt).FirstOrDefault();

            bool shouldSendEmail = false;
            string emailSubject = string.Empty;
            string emailBody = string.Empty;

            if (log.Status == 0 && (lastLog == null || lastLog.Status == 1))
            {
                shouldSendEmail = true;
                emailSubject = $"[CẢNH BÁO SẬP MẠNG] Dịch vụ [{service.ServiceName}] đã bị OFFLINE!";
            }
            else if (log.Status == 1 && lastLog != null && lastLog.Status == 0)
            {
                shouldSendEmail = true;
                emailSubject = $"[CẢNH BÁO KHÔI PHỤC] Dịch vụ [{service.ServiceName}] đã hoạt động trở lại!";
            }
            if (shouldSendEmail)
            {
                var project = await projectRepository.GetByIdAsync(service.ProjectId);
                if (project != null && !string.IsNullOrEmpty(project.OwnerId))
                {
                    var owner = await userRepository.GetByIdAsync(project.OwnerId);
                    if (owner != null && !string.IsNullOrEmpty(owner.Email))
                    {
                        var statusLabel = log.Status == 1 ? "ONLINE(Đang hoạt động)" : "OFFLINE(Đã bị sập)";
                        var statusColor = log.Status == 1 ? "green" : "red";
                        emailBody = $@" <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 8px;'>
                                <h2 style='color: {statusColor}; text-align: center;'>HỆ THỐNG CẢNH BÁO GIÁM SÁT</h2>
                                <p>Xin chào <b>{owner.Username}</b>,</p>
                                <p>Hệ thống giám sát vừa phát hiện sự thay đổi trạng thái của dịch vụ trực thuộc dự án của bạn:</p>
                                <table style='width: 100%; border-collapse: collapse;'>
                                    <tr style='background-color: #f9f9f9;'>
                                        <td style='padding: 8px; font-weight: bold; width: 150px;'>Dịch vụ:</td>
                                        <td style='padding: 8px;'>{service.ServiceName}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; font-weight: bold;'>Địa chỉ:</td>
                                        <td style='padding: 8px;'>{service.Ip}:{service.Port}</td>
                                    </tr>
                                    <tr style='background-color: #f9f9f9;'>
                                        <td style='padding: 8px; font-weight: bold;'>Giao thức:</td>
                                        <td style='padding: 8px;'>{service.Protocol.ToUpper()}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; font-weight: bold;'>Trạng thái:</td>
                                        <td style='padding: 8px; font-weight: bold; color: {statusColor};'>{statusLabel}</td>
                                    </tr>
                                    <tr style='background-color: #f9f9f9;'>
                                        <td style='padding: 8px; font-weight: bold;'>Thời gian phản hồi:</td>
                                        <td style='padding: 8px;'>{log.ResponseTimeMs} ms</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 8px; font-weight: bold;'>Mã lỗi/Phản hồi:</td>
                                        <td style='padding: 8px;'>{log.StatusCode}</td>
                                    </tr>
                                    <tr style='background-color: #f9f9f9;'>
                                        <td style='padding: 8px; font-weight: bold;'>Chi tiết thông điệp:</td>
                                        <td style='padding: 8px; color: #555;'><i>{log.ErrorMassage}</i></td>
                                    </tr>
                                </table>
                                <br/>
                                <p style='text-align: center; color: #999; font-size: 12px;'>Báo cáo tự động từ Hệ thống Giám sát Realtime của bạn lúc {DateTime.UtcNow.AddHours(7):dd/MM/yyyy HH:mm:ss} (GMT+7).</p>
                            </div>";
                        await emailService.SendEmailRegisterAsync(owner.Email, emailSubject, emailBody);
                        _logger.LogInformation("Alert Email sent successfully to user {Email} for service {ServiceName}", owner.Email, service.ServiceName);
                    }
                }
            }
        }
        catch (Exception alertEx)
        {
            _logger.LogError(alertEx, "Failed to evaluate alert check or send Email to project owner");
        }
        try
        {
            await monitorLogRepository.InsertOneAsync(log);
            _logger.LogInformation("Service [{ServiceName}] ({Ip}: {Port}) Checked.Status: {Status} | Time: {Time}ms",
            service.ServiceName,
            service.Ip, service.Port, log.Status == 1 ? "ONLINE" : "OFFLINE", log.ResponseTimeMs);
        }
        catch (Exception dbEx)
        {
            _logger.LogError(dbEx, "Failed to save monitor log for service {ServiceId}", service.Id);
        }
    }

}
