using Dirassati_Backend.Common.Extensions;
using Dirassati_Backend.Common.Services;
using Dirassati_Backend.Data;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Features.Students.DTOs;
using Dirassati_Backend.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Features.Students.Services;

public class ParentServices(AppDbContext dbContext, UserManager<AppUser> userManager, LinkGenerator linkGenerator, IHttpContextAccessor httpContext, IEmailService emailService)
{
    public string VerificationToken { get; private set; } = "";
    public string Email { get; private set; } = "";

    public async Task<Guid> RegisterParent(string NationalIdentityNumber, ParentInfosDto parentInfosDTO)
    {
        var parent = await dbContext.Parents
            .FirstOrDefaultAsync(p => p.NationalIdentityNumber == NationalIdentityNumber);

        if (parent != null)
            return parent.ParentId;


        var user = new AppUser
        {
            UserName = parentInfosDTO.Email,
            FirstName = parentInfosDTO.FirstName,
            LastName = parentInfosDTO.LastName,
            Email = parentInfosDTO.Email,
            PhoneNumber = parentInfosDTO.PhoneNumber,
            EmailConfirmed = false
        };
        var result = await userManager.CreateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Can not create user \n{result.Errors.ToCustomString()}");
        parent = new Parent
        {
            NationalIdentityNumber = parentInfosDTO.NationalIdentityNumber,
            Occupation = parentInfosDTO.Occupation,
            UserId = user.Id,
            User = user,
        };

        await dbContext.Parents.AddAsync(parent);

        await dbContext.SaveChangesAsync();

        var verificationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        VerificationToken = verificationToken;
        Email = user.Email;
        return parent.ParentId;

    }
    public async Task SendConfirmationEmailAsync()
    {
        var link = linkGenerator.GetUriByName(httpContext.HttpContext!, "VerifyEmail", new { Email, VerificationToken }) ?? throw new InvalidOperationException("Can't create verification email link");
        var body = $"Please Verify your email by clicking on <a href=\"{link}\">this link</a>";
        await emailService.SendEmailAsync(Email, "Confirmation Email", body, null, null, isHTML: true);
    }
}