using System.Security.Claims;
using Dirassati_Backend.Persistence;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Common.Services.EmailService;

public class EmailServices(IFluentEmail fluentEmail, IHttpContextAccessor httpContextAccessor, AppDbContext dbContext, ILogger<EmailServices> logger) : IEmailService
{
    private readonly IFluentEmail _fluentEmail = fluentEmail;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly AppDbContext _dbContext = dbContext;
    private readonly ILogger<EmailServices> _logger = logger;
    private string? _schoolEmail;
    private string? _schoolName;

    private async Task<bool> InitializeSchoolInfoAsync()
    {
        if (_schoolEmail != null) return false;

        var schoolId = _httpContextAccessor.HttpContext?.User.FindFirstValue("SchoolId");
        if (schoolId == null || !Guid.TryParse(schoolId, out var schoolIdGuid))
        {
            _logger.LogWarning("Unable to get valid school ID from claims");
            return true;
        }

        try
        {
            var school = await _dbContext.Schools
                .FirstOrDefaultAsync(s => s.SchoolId == schoolIdGuid);

            if (school == null)
            {
                _logger.LogError("School not found for ID: {SchoolId}", schoolId);
                throw new InvalidOperationException("School not found");
            }

            _schoolEmail = school.Email;
            _schoolName = school.Name;
            _logger.LogInformation("School info initialized: {SchoolName} ({SchoolEmail})", _schoolName, _schoolEmail);
            return false;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Error retrieving school information for ID: {SchoolId}", schoolId);
            throw;
        }
    }

    public async Task SendEmailAsync(string to, string subject, string body, string? fromName, string? fromEmail, bool isHTML = false)
    {
        try
        {
            await SendEmailWithTemplateAsync(to, subject, body, new EmailTemplateOptions(), fromName, fromEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {Recipient}", to);
            throw;
        }
    }

    public async Task SendEmailWithTemplateAsync(string to, string subject, string body, EmailTemplateOptions options, string? fromName = null, string? fromEmail = null)
    {
        try
        {
            _logger.LogInformation("Preparing to send email to {Recipient} with subject: {Subject}", to, subject);

            bool isEmailFromApp = false;
            if (fromName == null && fromEmail == null)
            {
                _logger.LogDebug("No sender specified, using school email");
                isEmailFromApp = await InitializeSchoolInfoAsync();
                if (!isEmailFromApp)
                {
                    _fluentEmail.SetFrom(_schoolEmail, _schoolName);
                    _logger.LogInformation("Using school as sender: {SchoolName} <{SchoolEmail}>", _schoolName, _schoolEmail);
                }
            }
            else
            {
                _fluentEmail.SetFrom(fromEmail, fromName);
                _logger.LogInformation("Using specified sender: {SenderName} <{SenderEmail}>", fromName ?? "(null)", fromEmail ?? "(null)");
            }

            var response = await _fluentEmail
                .To(to)
                .Subject(subject)
                .Body(body, true) // Always use HTML since we're implementing templated emails
                .SendAsync();

            if (response.Successful)
            {
                _logger.LogInformation("Email sent successfully to {Recipient}", to);
            }
            else
            {
                string errors = string.Join(", ", response.ErrorMessages);
                _logger.LogError("Failed to send email to {Recipient}: {ErrorMessages}", to, errors);
                throw new InvalidOperationException($"Failed to send email: {errors}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceptioclearn occurred while sending email to {Recipient}", to);
            throw;
        }
    }
}