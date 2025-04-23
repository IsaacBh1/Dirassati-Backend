namespace Dirassati_Backend.Common.Services.EmailService;

/// <summary>
/// Defines a contract for an enhanced email service with templating capabilities.
/// </summary>
public interface ITemplatedEmailService : IEmailService
{
    /// <summary>
    /// Sends an email with explicit header content for the template.
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body content</param>
    /// <param name="header">Custom header text to display in the email template</param>
    /// <param name="footer">Optional footer text to display in the email template</param>
    /// <param name="fromName">Optional sender name</param>
    /// <param name="fromEmail">Optional sender email</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task SendTemplatedEmailAsync(string to, string subject, string body, string header, string? footer = null, string? fromName = null, string? fromEmail = null);
}
