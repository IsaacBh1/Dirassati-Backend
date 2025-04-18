using Dirassati_Backend.Common;
using Dirassati_Backend.Common.Extensions;
using Dirassati_Backend.Data;
using Microsoft.AspNetCore.Identity;

namespace Dirassati_Backend.Features.Auth.Register.Services;

public class VerifyEmailService(UserManager<AppUser> _userManager)
{

    public async Task<Result<string, string>> VerifyEmailAsync(string email, string token)
    {
        var result = new Result<string, string>();
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return result.Failure("User does not exist", 404);

        var decodedToken = Uri.UnescapeDataString(token);

        var isVerified = await _userManager.ConfirmEmailAsync(user, decodedToken);

        return isVerified.Succeeded ?
            result.Success("Your email is verified") :
            result.Failure($"Cannot Verify Email\n {isVerified.Errors.ToCustomString()} ", 500);
    }
}
