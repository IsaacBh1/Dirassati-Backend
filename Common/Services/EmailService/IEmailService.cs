namespace Dirassati_Backend.Common.Services;

/// <summary>
/// Defines a contract for an email service that provides functionality to send emails.
/// </summary>
public interface IEmailService
{

    public Task SendEmailAsync(string to, string subject, string body, string? fromName, string? fromEmail, bool isHTML = false);
}
