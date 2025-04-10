namespace Dirassati_Backend.Common.Services;

/// <summary>
/// Defines a contract for an email service that provides functionality to send emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="fromName">The name of the sender (optional).</param>
    /// <param name="fromEmail">The email address of the sender (optional).</param>
    /// <param name="isHTML">Indicates whether the email body is in HTML format. Defaults to <c>false</c>.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendEmailAsync(string to, string subject, string body, string? fromName, string? fromEmail, bool isHTML = false);
}

