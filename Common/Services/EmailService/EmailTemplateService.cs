using System.Security.Claims;
using Dirassati_Backend.Persistence;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Fluid;
using System.IO;

namespace Dirassati_Backend.Common.Services.EmailService;

public class EmailTemplateService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<EmailTemplateService> _logger;
    private readonly FluidParser _parser;
    private readonly string _templatePath;

    private string? _schoolEmail;
    private string? _schoolName;

    public EmailTemplateService(
        IFluentEmail fluentEmail,
        IHttpContextAccessor httpContextAccessor,
        AppDbContext dbContext,
        ILogger<EmailTemplateService> logger,
        IWebHostEnvironment hostEnvironment)
    {
        _fluentEmail = fluentEmail;
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
        _logger = logger;
        _parser = new FluidParser();
        _templatePath = Path.Combine(hostEnvironment.ContentRootPath, "Common", "Services", "EmailService", "Templates", "EmailTemplate.liquid");
    }

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

    private async Task<string> ApplyTemplateAsync(string body, string header, EmailTemplateOptions options)
    {
        try
        {
            if (!File.Exists(_templatePath))
            {
                _logger.LogWarning("Email template not found at path: {TemplatePath}", _templatePath);
                return body;
            }

            string templateContent = await File.ReadAllTextAsync(_templatePath);

            if (!_parser.TryParse(templateContent, out var template, out var errors))
            {
                _logger.LogError("Failed to parse email template: {Errors}", string.Join(", ", errors));
                return body;
            }

            var templateContext = new TemplateContext();
            templateContext.SetValue("header", header);
            templateContext.SetValue("body", new Fluid.Values.StringValue(body, true)); // Allow HTML in body
            templateContext.SetValue("footer", options.Footer ?? "");
            templateContext.SetValue("title", options.Title ?? header);
            templateContext.SetValue("currentYear", DateTime.Now.Year.ToString());

            // Add CTA button if specified
            if (!string.IsNullOrEmpty(options.CtaText) && !string.IsNullOrEmpty(options.CtaUrl))
            {
                templateContext.SetValue("cta_text", options.CtaText);
                templateContext.SetValue("cta_url", options.CtaUrl);
            }

            return await template.RenderAsync(templateContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying email template");
            return body; // Fallback to original content
        }
    }

    public async Task SendEmailAsync(string to, string subject, string body, string? fromName = null, string? fromEmail = null, bool isHTML = false)
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

    public async Task SendEmailWithTemplateAsync(
        string to,
        string subject,
        string body,
        EmailTemplateOptions options,
        string? fromName = null,
        string? fromEmail = null)
    {
        try
        {
            _logger.LogInformation("Preparing to send templated email to {Recipient} with subject: {Subject}", to, subject);

            // Always render as HTML since we're using a template
            string templateHeader = options.Header ?? subject;
            string formattedBody = await ApplyTemplateAsync(body, templateHeader, options);

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

            var emailBuilder = _fluentEmail
                .To(to)
                .Subject(subject)
                .Body(formattedBody, true); // Always true as we're using HTML template

            var response = await emailBuilder.SendAsync();

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
            _logger.LogError(ex, "Exception occurred while sending email to {Recipient}", to);
            throw;
        }
    }
}

public class EmailTemplateOptions
{
    /// <summary>
    /// Custom header text to display at the top of the email
    /// </summary>
    public string? Header { get; set; }

    /// <summary>
    /// Title for the email (used in HTML title tag)
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Footer text to display at the bottom of the email
    /// </summary>
    public string? Footer { get; set; }

    /// <summary>
    /// Text for call-to-action button
    /// </summary>
    public string? CtaText { get; set; }

    /// <summary>
    /// URL for call-to-action button
    /// </summary>
    public string? CtaUrl { get; set; }
}
