namespace backend.Services.Interfaces;
public interface IEmailService
{
    Task SendEmailRegisterAsync(string toEmail, string subject, string htmlBody);
    Task SendEmailForgotPasswordAsync(string toEmail, string subject, string htmlBody);
    Task SendAlertEmailAsync(string toEmail, string serviceName, string ip, int port, string newStatus, string message);
}