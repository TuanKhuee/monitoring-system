using backend.Services.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using backend.Settings;

namespace backend.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;

    }

    public async Task SendEmailRegisterAsync(string toEmail, string subject, string htmlBody)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder{ HtmlBody = htmlBody};
        email.Body = builder.ToMessageBody();
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);

        await smtp.SendAsync(email);

        await smtp.DisconnectAsync(true);
    }
    
    public Task SendEmailForgotPasswordAsync(string toEmail, string subject, string htmlBody)
    {
        throw new NotImplementedException();
    }

    public async Task SendAlertEmailAsync(string toEmail, string serviceName, string ip, int port, string newStatus, string message)
    {
        var isOnline = newStatus == "ONLINE";
        var statusColor = isOnline ? "#10b981" : "#ef4444";
        var statusIcon = isOnline ? "ONLINE" : "OFFLINE";
        var subject = isOnline 
            ? $" [RECOVERED] Dịch vụ {serviceName} đã hoạt động trở lại" 
            : $" [ALERT] Dịch vụ {serviceName} đã ngừng hoạt động!";

        var htmlBody = $@"
        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; background: #0f172a; border-radius: 16px; overflow: hidden; border: 1px solid #334155;'>
            <div style='background: linear-gradient(135deg, {(isOnline ? "#065f46" : "#7f1d1d")}, #1e293b); padding: 32px; text-align: center;'>
                <span style='font-size: 48px; color: white;'>{statusIcon}</span>
                <h1 style='color: white; margin: 16px 0 8px; font-size: 22px;'>Trạng thái dịch vụ đã thay đổi</h1>
                <p style='color: #94a3b8; margin: 0; font-size: 14px;'>Monitoring System Alert</p>
            </div>
            <div style='padding: 32px;'>
                <table style='width: 100%; border-collapse: collapse;'>
                    <tr>
                        <td style='padding: 12px 16px; color: #94a3b8; font-size: 13px; border-bottom: 1px solid #1e293b;'>Dịch vụ</td>
                        <td style='padding: 12px 16px; color: white; font-size: 13px; font-weight: bold; border-bottom: 1px solid #1e293b; text-align: right;'>{serviceName}</td>
                    </tr>
                    <tr>
                        <td style='padding: 12px 16px; color: #94a3b8; font-size: 13px; border-bottom: 1px solid #1e293b;'>Địa chỉ</td>
                        <td style='padding: 12px 16px; color: white; font-size: 13px; font-weight: bold; border-bottom: 1px solid #1e293b; text-align: right;'>{ip}:{port}</td>
                    </tr>
                    <tr>
                        <td style='padding: 12px 16px; color: #94a3b8; font-size: 13px; border-bottom: 1px solid #1e293b;'>Trạng thái mới</td>
                        <td style='padding: 12px 16px; font-size: 13px; font-weight: bold; border-bottom: 1px solid #1e293b; text-align: right;'>
                            <span style='color: {statusColor}; background: {(isOnline ? "#064e3b" : "#450a0a")}; padding: 4px 12px; border-radius: 6px;'>{newStatus}</span>
                        </td>
                    </tr>
                    <tr>
                        <td style='padding: 12px 16px; color: #94a3b8; font-size: 13px; border-bottom: 1px solid #1e293b;'>Chi tiết</td>
                        <td style='padding: 12px 16px; color: #fbbf24; font-size: 13px; border-bottom: 1px solid #1e293b; text-align: right;'>{message}</td>
                    </tr>
                    <tr>
                        <td style='padding: 12px 16px; color: #94a3b8; font-size: 13px;'>Thời gian</td>
                        <td style='padding: 12px 16px; color: white; font-size: 13px; text-align: right;'>{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</td>
                    </tr>
                </table>
            </div>
            <div style='padding: 16px 32px 24px; text-align: center; color: #64748b; font-size: 11px;'>
                <p>Email này được gửi tự động từ hệ thống Monitoring System.</p>
            </div>
        </div>";

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}