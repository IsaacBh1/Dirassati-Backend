using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
namespace Dirassati_Backend.Common.Security;

public class TokenProvider(IConfiguration configuration )
{
 // Default to 15 days if not configured
       public string GenerateJwtToken(List<Claim> claims)
       {
              var jwtKey = configuration["Jwt:Key"]
                           ?? throw new InvalidOperationException("Jwt:Key configuration is missing");
              var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
              var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
              var token = new JwtSecurityToken(
                     issuer: configuration["Jwt:Issuer"],
                     audience: configuration["Jwt:Audience"],
                     claims: claims,
                     expires: DateTime.UtcNow.AddDays(int.TryParse( configuration["Jwt:AccessTokenExpiry"] ?? "15",  out var accessTokenExpiry) ? accessTokenExpiry : throw new ArgumentNullException(nameof(accessTokenExpiry),"Access Token Expiry is not set")),
                     signingCredentials: creds
              );
              return new JwtSecurityTokenHandler().WriteToken(token);
       }

       public string GenerateRefreshToken()
       {
              return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
       }
     
}