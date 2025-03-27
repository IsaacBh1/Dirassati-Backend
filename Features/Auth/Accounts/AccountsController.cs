using System.Security.Claims;
using Dirassati_Backend.Features.Auth.Accounts.DTOs;
using Dirassati_Backend.Features.Auth.Accounts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Auth.Accounts;

[Route("api/[controller]")]
[ApiController]
public class AccountsController(AccountServices accountServices, UserManager<AppUser> userManager) : BaseController
{
    private readonly AccountServices _accountServices = accountServices;
    private readonly UserManager<AppUser> _userManager = userManager;

    // [HttpPost("request-password-reset")]
    // public async Task<ActionResult> ReqeuestPasswordReset(RequestPasswordResetDTO requestPasswordResetDTO)
    // {
    //     var userType = User.FindFirstValue("UserType");
    //     if (userType is null)
    //         return Unauthorized();
    //     if (userType.Equals("employee", StringComparison.CurrentCultureIgnoreCase) || userType.Equals("teacher", StringComparison.CurrentCultureIgnoreCase))
    //     {
    //         var result = await _accountServices.UpdateUserPassword(userType);

    //     }
    //     else
    //         return Unauthorized();
    //     return Ok();

    // }

    // [HttpPut("update-password")]
    // public async Task<ActionResult> ResetPassword(ResetPasswordDTO passwordDTO)
    // {
    //     var userType = User.FindFirstValue("UserType");
    //     if (userType is null)
    //         return Unauthorized();
    //     if (userType.Equals("employee", StringComparison.CurrentCultureIgnoreCase) || userType.Equals("teacher", StringComparison.CurrentCultureIgnoreCase))
    //     {
    //         var result = await _accountServices.UpdateUserPassword(userType);
    //     }
    //     else
    //         return Unauthorized();


    // }


    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new { message = "User not found or session expired." });

        var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.OldPassword, changePasswordDto.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "Password change failed.",
                errors = result.Errors
            });
        }
        await _userManager.UpdateSecurityStampAsync(user);
        return Ok(new { message = "Password changed successfully." });
    }

}

