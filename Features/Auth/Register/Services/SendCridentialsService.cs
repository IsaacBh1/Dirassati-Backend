using Dirassati_Backend.Common;
using Dirassati_Backend.Common.Extensions;
using Dirassati_Backend.Common.Services;
using Dirassati_Backend.Data;
using Microsoft.AspNetCore.Identity;

namespace Dirassati_Backend.Features.Auth.Register.Services;

public class SendCridentialsService(IEmailService emailService, UserManager<AppUser> userManager)
{
    private readonly IEmailService _emailService = emailService;
    private readonly UserManager<AppUser> _userManager = userManager;

    public async Task<Result<string, string>> SendCridentialsAsync(string email)
    {
        var result = new Result<string, string>();
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return result.Failure("User Does not Exist", 404);
        if (!user.EmailConfirmed)
            return result.Failure("Email Is Not Confirmed", 400);
        var password = PasswordGeneratorService.GeneratePassword(15);
        try
        {
            await SendEmailAsync(email, password);
            var AddPasswordResult = await _userManager.AddPasswordAsync(user, password);
            if (!AddPasswordResult.Succeeded)
                return result.Failure($"Failed to reset password \n {AddPasswordResult.Errors.ToCustomString()}", 500);
            return result.Success("Your credentials have been sent to your email");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return result.Failure(ex.Message, 500);
        }
    }
    
    private async Task SendEmailAsync(string email, string password)
    {
        var body = $@"
            <h1style='color:DarkCyan;'>Welcome to Dirassati</h1>
            <p>Your account credentials are:</p>
            <p>Email: {email}</p>
            <p>Password: {System.Web.HttpUtility.HtmlEncode(password)}</p>";

        await _emailService.SendEmailAsync(email, "Your Dirassati Account Credentials", body, null, null, true);
    }
}
