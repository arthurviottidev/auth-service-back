using AuthService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace AuthService.Infrastructure.Services;

public class EmailService(IConfiguration configuration) : IEmailService
{
    private readonly IConfigurationSection _email = configuration.GetSection("Email");

    public async Task SendPasswordResetEmailAsync(string toEmail, string token)
    {
        var resetLink = $"http://localhost:4200/reset-password?token={token}";

        var body = $"""
            <h2>Recuperação de senha</h2>
            <p>Clique no link abaixo para redefinir sua senha. O link expira em 2 horas.</p>
            <a href="{resetLink}">Redefinir senha</a>
            """;

        await SendAsync(toEmail, "Recuperação de senha", body);
    }

    private async Task SendAsync(string to, string subject, string body)
    {
        using var client = new SmtpClient(_email["SmtpHost"])
        {
            Port = int.Parse(_email["SmtpPort"]!),
            Credentials = new NetworkCredential(_email["SenderEmail"], _email["SenderPassword"]),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(_email["SenderEmail"]!),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(to);
        await client.SendMailAsync(message);
    }
}