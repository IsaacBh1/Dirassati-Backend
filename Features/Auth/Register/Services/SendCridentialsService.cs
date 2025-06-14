using Dirassati_Backend.Common;
using Dirassati_Backend.Common.Extensions;
using Dirassati_Backend.Common.Services.EmailService;
using Dirassati_Backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Dirassati_Backend.Features.Auth.Register.Services;

public class SendCridentialsService(IEmailService emailService, UserManager<AppUser> userManager, ILogger<SendCridentialsService> logger)
{
    private readonly IEmailService _emailService = emailService;
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly ILogger<SendCridentialsService> _logger = logger;

    public async Task<Result<string, string>> SendCridentialsAsync(string email)
    {
        _logger.LogInformation("Attempting to send credentials for email: {Email}", email);

        var result = new Result<string, string>();
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            _logger.LogWarning("User not found for email: {Email}", email);
            return result.Failure("User Does not Exist", 404);
        }

        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("Email not confirmed for user: {Email}", email);
            return result.Failure("Email Is Not Confirmed", 400);
        }

        var password = PasswordGeneratorService.GeneratePassword(15);
        try
        {
            await SendEmailAsync(email, password);
            _logger.LogInformation("Credentials email sent successfully to: {Email}", email);

            var AddPasswordResult = await _userManager.AddPasswordAsync(user, password);
            if (!AddPasswordResult.Succeeded)
            {
                _logger.LogError("Failed to add password for user {Email}. Errors: {Errors}",
                    email, AddPasswordResult.Errors.ToCustomString());
                return result.Failure($"Failed to reset password \n {AddPasswordResult.Errors.ToCustomString()}", 500);
            }

            _logger.LogInformation("Credentials sent successfully for user: {Email}", email);
            return result.Success("Your credentials have been sent to your email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending credentials for email: {Email}", email);
            return result.Failure(ex.Message, 500);
        }
    }

    private async Task SendEmailAsync(string email, string password)
    {
        _logger.LogDebug("Preparing to send credentials email to: {Email}", email);

        var body = $@"
            <h1style='color:DarkCyan;'>Welcome to Dirassati</h1>
            <p>Your account credentials are:</p>
            <p>Email: {email}</p>
            <p>Password: {System.Web.HttpUtility.HtmlEncode(password)}</p>";

        var options = new EmailTemplateOptions
        {
            Header = "Your Dirassati Account Credentials",
            Footer = "Dirassati - Your Educational Partner"
        };

        await _emailService.SendEmailWithTemplateAsync(
            email,
            "Your Dirassati Account Credentials",
            body,
            options,
            "Dirassati Team",
            "noreply@dirassati.com"
        );

        _logger.LogDebug("Email template sent for: {Email}", email);
    }
}
