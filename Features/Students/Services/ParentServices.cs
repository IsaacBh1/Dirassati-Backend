using Dirassati_Backend.Common.Extensions;
using Dirassati_Backend.Common.Services;
using Dirassati_Backend.Features.Students.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Dirassati_Backend.Features.Students.Services;

public class ParentServices(AppDbContext dbContext, UserManager<AppUser> userManager, LinkGenerator linkGenerator, IHttpContextAccessor httpContext, IEmailService emailService)
{
    public async Task<Guid> RegisterParent(string NationalIdentityNumber, ParentInfosDTO parentInfosDTO)
    {
        var parent = await dbContext.Parents
            .FirstOrDefaultAsync(p => p.NationalIdentityNumber == NationalIdentityNumber);

        if (parent != null)
            return parent.ParentId;


        try
        {
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
                throw new Exception($"Can not create user \n{result.Errors.ToCustomString()}");
            parent = new Domain.Models.Parent
            {
                NationalIdentityNumber = parentInfosDTO.NationalIdentityNumber,
                Occupation = parentInfosDTO.Occupation,
                UserId = user.Id,
                User = user,
            };

            await dbContext.Parents.AddAsync(parent);

            await dbContext.SaveChangesAsync();

            var verificationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await SendConfirmationEmailAsync(verificationToken, parentInfosDTO.Email);
            return parent.ParentId;
        }
        catch (Exception)
        {

            throw;
        }
    }
    private async Task SendConfirmationEmailAsync(string token, string email)
    {
        var link = linkGenerator.GetUriByName(httpContext.HttpContext!, "VerifyEmail", new { email, token }) ?? throw new Exception("Can't create verification email link");
        var body = $"Please Verify your email by clicking on <a href=\"{link}\">this link</a>";
        await emailService.SendEmailAsync(email, "Confirmation Email", body, null, null, isHTML: true);
    }
}