using System.IdentityModel.Tokens.Jwt;
using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Common.Repositories;

namespace Dirassati_Backend.Common.Security;

public class RefreshTokenProvider(TokenProvider tokenProvider, IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration)
{
    public async Task<Result<RefreshTokenResponseDto, string>> GenerateNewJwtFromRefreshToken(string refreshToken,string accessToken)
    {
        var result = new Result<RefreshTokenResponseDto, string>();
        var refreshTokenFromDb =await refreshTokenRepository.FindByTokenAsync(refreshToken);
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            // Check if the refresh token exists and is not expired or if the current jwt token is still valid 
            if (refreshTokenFromDb is null || refreshTokenFromDb.ExpiresOn < DateTime.UtcNow || jwtToken.ValidTo >= DateTime.UtcNow)
            {
                return result.Failure("Invalid or expired refresh token",401);
            }
            var token = new JwtSecurityToken(
                issuer: jwtToken.Issuer,
                audience: jwtToken.Audiences.First(),
                claims: jwtToken.Claims.ToList(),
                expires: DateTime.UtcNow.AddDays(int.TryParse( configuration["Jwt:AccessTokenExpiry"] ?? "15",  out var accessTokenExpiry) ? accessTokenExpiry : throw new ArgumentNullException(nameof(accessTokenExpiry),"Access Token Expiry is not set")),
                signingCredentials: jwtToken.SigningCredentials
            );
            var newRefreshToken = await refreshTokenRepository.AddNewRefreshTokenAsync(refreshTokenFromDb.UserId);
            return result.Success(new RefreshTokenResponseDto
            {
                RefreshToken = newRefreshToken,
                AccessToken = handler.WriteToken(token)
                     
            });

        }
        catch (Exception e)
        {
            return result.Failure("Invalid Access Token",401);
            throw;
        }
       
    }
}