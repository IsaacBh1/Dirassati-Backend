using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dirassati_Backend.Features.Teachers.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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
    public class TeacherAuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public TeacherAuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] TeacherLoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return Unauthorized("Invalid email or password");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid email or password");

            var teacher = await _context.Teachers
                .Include(t => t.School)
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher == null)
                return Unauthorized("Teacher not found");

            var token = GenerateJwtToken(user, teacher.SchoolId.ToString());
            return Ok(new
            {
                Token = token,
                user.FirstName,
                user.LastName,
                teacher.SchoolId
            });
        }

        private string GenerateJwtToken(AppUser user, string schoolId)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("SchoolId", schoolId),
                new Claim(ClaimTypes.Role, "Teacher")
            };

            var jwtKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key configuration is missing");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(15),
                signingCredentials: creds
            );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}