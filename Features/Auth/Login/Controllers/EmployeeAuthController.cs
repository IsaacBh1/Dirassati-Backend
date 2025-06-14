using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Common.Repositories;
using Dirassati_Backend.Common.Security;
using Dirassati_Backend.Dtos;
using Microsoft.EntityFrameworkCore;
using Dirassati_Backend.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Dirassati_Backend.Data;
using Dirassati_Backend.Persistence;

namespace Dirassati_Backend.Features.Auth.Login.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Tags("Employee Authentication")]
    [AllowAnonymous]
    public class EmployeeAuthController(
        IRefreshTokenRepository refreshTokenRepository,
        RefreshTokenProvider refreshTokenProvider,
        TokenProvider tokenProvider,
        UserManager<AppUser> userManager,
        AppDbContext context)
        : ControllerBase
    {
        public sealed class RefreshTokenDto
        {
            public required string refreshToken { get; set; }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] EmployeeLoginDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            var passwordValid = await userManager.CheckPasswordAsync(user, dto.Password);
            if (!passwordValid)
            {
                return Unauthorized("Invalid email or password");
            }

            if (!Guid.TryParse(user.Id, out Guid userGuid))
            {
                return Unauthorized("User identifier is not valid");
            }

            // Compare by converting userGuid to string
            var employee = await context.Employees
                .Include(e => e.School)
                .FirstOrDefaultAsync(e => e.UserId == userGuid.ToString());
            if (employee == null)
            {
                return Unauthorized("Employee record not found.");
            }

            var token = GenerateJwtToken(user, employee);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Failed to generate token.");
            }

            var refreshToken = await refreshTokenRepository.AddNewRefreshTokenAsync(employee.UserId);
            return Ok(new { Token = token, user.FirstName, user.LastName, RefreshToken = refreshToken });
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken(RefreshTokenDto dto, [FromHeader(Name = "Authorization")] string authorization)
        {
            var accessToken = authorization.Replace("Bearer ", "");
            var result = await refreshTokenProvider.GenerateNewJwtFromRefreshToken(dto.refreshToken, accessToken);
            if (!result.IsSuccess)
            {
                return Unauthorized(result.Errors);
            }
            return Ok(result.Value);
        }
        private string GenerateJwtToken(AppUser user, Employee employee)
        {

            var claims = new List<Claim>
    {
        new(JwtRegisteredClaimNames.Sub, user.Id),
        new(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new("EmployeeId", employee.EmployeeId.ToString()),
        new("Permission", employee.Permissions.ToString()),
        new("SchoolId", employee.SchoolId.ToString().ToLower()),
        new("SchoolTypeId" , employee.School.SchoolTypeId.ToString().ToUpper()),
        new(ClaimTypes.Role, "Employee")

    };
            return tokenProvider.GenerateJwtToken(claims);
        }
    }
}