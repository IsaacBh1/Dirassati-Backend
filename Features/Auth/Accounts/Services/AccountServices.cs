using Dirassati_Backend.Common;
using Dirassati_Backend.Common.Extensions;
using Dirassati_Backend.Common.Services.EmailService;
using Dirassati_Backend.Data;
using Dirassati_Backend.Features.Auth.Accounts.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Dirassati_Backend.Features.Auth.Accounts.Services;

public class AccountServices(UserManager<AppUser> userManager, IEmailService emailService, IConfiguration configurationManager)
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IEmailService _emailService = emailService;
    private readonly IConfiguration _configurationManager = configurationManager;

    public async Task<Result<string, string>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var result = new Result<string, string>();

        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user == null) return result.Failure("User Not Found", 404);

        var resetResult = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
        if (!resetResult.Succeeded)
        {

            Console.WriteLine(resetResult.Errors.ToCustomString());
            return result.Failure($"Error Happend while resetting password:\n {resetResult.Errors.ToCustomString()}", 400);
        }
        return result.Success("Password has been updated Successfully");
    }


    public async Task<Result<string, string>> SendResetPasswordTokenAsync(RequestPasswordResetDto resetDTO)
    {
        var result = new Result<string, string>();
        try
        {
            var user = await _userManager.FindByEmailAsync(resetDTO.Email);
            if (user is null)
                return result.Failure("User Not Found", 404);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await SendResetPasswordEmailAsync(user.Email!, token);
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.WriteLine($"The Token => {token}");
            Console.ResetColor();
            return result.Success("Reset Email sent successfully");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return result.Failure($"Error happend while sending the email {e.Message} : ", 500);

        }


    }

    private async Task SendResetPasswordEmailAsync(string email, string token)
    {
        var frontendUrl = _configurationManager["FrontendUrl"] ?? throw new InvalidOperationException("Can't find front end url");
        var link = $"{frontendUrl}/reset-password?token={token}&email={email}";

        var body = $"Click on this link to reset your password <a href=\"{link}\">this link</a>\nThe reset link is valid for 1 hour and <b>DON'T SHARE THIS LINK WITH ANYONE</b>";
        await _emailService.SendEmailAsync(email, "Confirmation Email", body, null, null, isHTML: true);
    }
}
