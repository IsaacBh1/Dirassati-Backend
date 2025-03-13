using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dirassati_Backend.Features.Parents.Dtos;
using Dirassati_Backend.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Persistence; // For EF Core extensions

[Route("api/parent/auth")]
[ApiController]
public class ParentAuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context; // i addede this to query for the parent 

    public ParentAuthController(UserManager<AppUser> userManager,
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
    public async Task<IActionResult> Login([FromBody] ParentLoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
            return Unauthorized("Invalid email or password");

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded)
            return Unauthorized("Invalid email or password");


        var parent = await _context.Parents.FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (parent == null)
            return Unauthorized("Parent not found");


        var token = GenerateJwtToken(user, parent.ParentId.ToString());
        return Ok(new { Token = token,parent.User.FirstName, parent.User.LastName });
    }

    private string GenerateJwtToken(AppUser user, string parentId)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("parentId", parentId) // Adding parentId claim
        };

        var jwtKey = _configuration["Jwt:Key"]
                     ?? throw new ArgumentNullException("Jwt:Key configuration is missing");
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
