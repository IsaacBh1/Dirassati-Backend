using System;
using System.Runtime.CompilerServices;
using Dirassati_Backend.Common;
using Dirassati_Backend.Features.Auth.Accounts.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dirassati_Backend.Features.Auth.Accounts.Services;

public class AccountServices(UserManager<AppUser> userManager)
{
    private readonly UserManager<AppUser> _userManager = userManager;

    //TODO:Implement rest password

    // public async Task<Result<Unit, string>> ResetPasswordAsync(string UserType)
    // {


    // }


    // public async Task<Result<string, string> SendResetPasswordTokenAsync(RequestPasswordResetDTO resetDTO)
    // {
    //     var result = new Result<string, string>();
    //     var user = await _userManager.FindByEmailAsync(resetDTO.Email);
    //     if (user is null)
    //         return result.Failure("User Not Found", 404);

    // }



}
