using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dirassati_Backend.Common.Repositories;
using Dirassati_Backend.Common.Security;
using Dirassati_Backend.Features.Parents.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Dirassati_Backend.Data;
using Dirassati_Backend.Persistence;

namespace Dirassati_Backend.Features.Parents.Controllers
{
    [Tags("Parent")]
    [Route("api/parent/auth")]
    [ApiController]
    [AllowAnonymous]
    public class ParentAuthController(
        IRefreshTokenRepository refreshTokenRepository,
        TokenProvider tokenProvider,
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        AppDbContext context)
        : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] ParentLoginDto loginDto)
        {
            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return Unauthorized("Invalid email or password");

            var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid email or password");


            var parent = await context.Parents.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (parent == null)
                return Unauthorized("Parent not found");


            var token = GenerateJwtToken(user, parent.ParentId.ToString());
            if (string.IsNullOrEmpty(token))
                return Unauthorized("Failed to generate token");
            // Save the refresh token
            var refreshToken = await refreshTokenRepository.AddNewRefreshTokenAsync(parent.UserId);
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized("Failed to generate refresh token");
            return Ok(new { Token = token, parent.User.FirstName, parent.User.LastName, refreshToken });
        }
        private string GenerateJwtToken(AppUser user, string parentId)
        {
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("parentId", parentId),
            new(ClaimTypes.Role, "Parent")

        };
            return tokenProvider.GenerateJwtToken(claims);
        }
    }
}