using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dirassati_Backend.Common.Repositories;
using Dirassati_Backend.Common.Security;
using Dirassati_Backend.Features.Teachers.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Dirassati_Backend.Data;
using Dirassati_Backend.Persistence;
namespace Dirassati_Backend.Features.Teachers.Controllers
{

    [Tags("Teacher")]
    [Route("api/teacher/auth")]
    [ApiController]
    [AllowAnonymous]
    public class TeacherAuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IConfiguration configuration,
        AppDbContext context,
        RefreshTokenRepository refreshTokenRepository,
        TokenProvider tokenProvider)
        : ControllerBase
    {
        private readonly IConfiguration _configuration = configuration;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] TeacherLoginDto loginDto)
        {
            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return Unauthorized("Invalid email or password");

            var result = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid email or password");

            var teacher = await context.Teachers
                .AsNoTracking()
                .Include(t => t.School)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher == null)
                return Unauthorized("Teacher not found");

            var token =GenerateJwtToken (user, teacher.SchoolId.ToString() , teacher.TeacherId.ToString());
            if (string.IsNullOrEmpty(token))
                return Unauthorized("Failed to generate token");
            //save the refresh token
            var refreshToken =await refreshTokenRepository.AddNewRefreshTokenAsync(teacher.UserId);
          
            return Ok(new 
            {
                RefreshToken = refreshToken,
                Token = token,
               user.FirstName,
               user.LastName,
              teacher.SchoolId
            });
        }

        private string GenerateJwtToken(AppUser user, string schoolId , string teacherId)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("TeacherId", teacherId),
                new Claim("SchoolId", schoolId),
                new Claim(ClaimTypes.Role, "Teacher")
            };
            return tokenProvider.GenerateJwtToken(claims);
        }
    }
}