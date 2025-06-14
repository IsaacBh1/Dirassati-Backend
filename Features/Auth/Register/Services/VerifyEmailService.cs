using Dirassati_Backend.Common;
using Dirassati_Backend.Common.Extensions;
using Dirassati_Backend.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Dirassati_Backend.Features.Auth.Register.Services;

public class VerifyEmailService(UserManager<AppUser> userManager, ILogger<VerifyEmailService> logger)
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly ILogger<VerifyEmailService> _logger = logger;

    public async Task<Result<string, string>> VerifyEmailAsync(string email, string token)
    {
        _logger.LogInformation("Attempting to verify email for user with email: {Email}", email);
        var result = new Result<string, string>();

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            _logger.LogWarning("Email verification failed: User with email {Email} does not exist", email);
            return result.Failure("User does not exist", 404);
        }

        var decodedToken = Uri.UnescapeDataString(token);
        _logger.LogDebug("Token decoded for email verification");

        var isVerified = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (isVerified.Succeeded)
        {
            _logger.LogInformation("Email verification successful for user {UserId}", user.Id);
            return result.Success("Your email is verified");
        }
        else
        {
            var errors = isVerified.Errors.ToCustomString();
            _logger.LogError("Email verification failed for user {UserId}. Errors: {Errors}", user.Id, errors);
            return result.Failure($"Cannot Verify Email\n {errors} ", 500);
        }
    }
}
