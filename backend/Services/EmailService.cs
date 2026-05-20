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
}