namespace Dirassati_Backend.Common.Services.EmailService;

/// <summary>
/// Defines a contract for an email service that provides functionality to send emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email to the specified recipient.
    /// </summary>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="fromName">The sender's display name (optional).</param>
    /// <param name="fromEmail">The sender's email address (optional).</param>
    /// <param name="isHTML">Indicates whether the body content is HTML.</param>
    Task SendEmailAsync(string to, string subject, string body, string? fromName, string? fromEmail, bool isHTML = false);

    /// <summary>
    /// Sends an email to the specified recipient using a customized template.
    /// </summary>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="options">Customization options for the email template.</param>
    /// <param name="fromName">The sender's display name (optional).</param>
    /// <param name="fromEmail">The sender's email address (optional).</param>
    Task SendEmailWithTemplateAsync(string to, string subject, string body, EmailTemplateOptions options, string? fromName = null, string? fromEmail = null);
}
