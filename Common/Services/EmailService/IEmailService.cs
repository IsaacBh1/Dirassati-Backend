namespace Dirassati_Backend.Common.Services;

public interface IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body, string? fromName, string? fromEmail, bool isHTML = false);
}