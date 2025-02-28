using Dirassati_Backend.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Persistence;
using Dirassati_Backend.Dtos;
[ApiController]
[Route("api/employee/auth")]
public class EmployeeAuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public EmployeeAuthController(UserManager<AppUser> userManager, AppDbContext context, IConfiguration configuration)
    {
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] EmployeeLoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return Unauthorized("Invalid email or password");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
        {
            return Unauthorized("Invalid email or password");
        }
        if (!Guid.TryParse(user.Id, out Guid userGuid))
        {
            return Unauthorized("User identifier is not valid");
        }

        var employee = _context.Employees.FirstOrDefault(e => e.UserId == userGuid);
        if (employee == null)  return Unauthorized("Employee record not found.");
        

        
        var token = GenerateJwtToken(user, employee);
        return Ok(new { Token = token });
    }

    private string GenerateJwtToken(AppUser user, Employee employee)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim("EmployeeId", employee.EmployeeId.ToString()),
            new Claim("Permission", employee.Permissions.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
