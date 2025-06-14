using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Dirassati_Backend.Common.Dtos;
using Dirassati_Backend.Common.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace Dirassati_Backend.Common.Security;

public class RefreshTokenProvider(IRefreshTokenRepository refreshTokenRepository, IConfiguration configuration, ILogger<TokenProvider> logger)
{
    public async Task<Result<RefreshTokenResponseDto, string>> GenerateNewJwtFromRefreshToken(string refreshToken, string accessToken)
    {
        var result = new Result<RefreshTokenResponseDto, string>();
        logger.LogInformation("Attempting to generate new JWT from refresh token for access token: {AccessToken}", accessToken);

        var refreshTokenFromDb = await refreshTokenRepository.FindByTokenAsync(refreshToken);
        if (refreshTokenFromDb == null)
        {
            logger.LogWarning("Refresh token not found in database: {RefreshToken}", refreshToken);
        }
        else
        {
            logger.LogInformation("Refresh token found for user ID: {UserId}", refreshTokenFromDb.UserId);
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);

            // Check if the refresh token exists and is not expired or if the current jwt token is still valid 
            if (refreshTokenFromDb is null || refreshTokenFromDb.ExpiresOn < DateTime.UtcNow || jwtToken.ValidTo >= DateTime.UtcNow)
            {
                logger.LogError("firstResult{Result}", refreshTokenFromDb?.ExpiresOn < DateTime.UtcNow);
                logger.LogError("seconfResult{Result}", jwtToken.ValidTo >= DateTime.UtcNow);
                logger.LogError("Now{Result}", DateTime.UtcNow);

                logger.LogWarning("Invalid or expired refresh token or JWT token still valid. RefreshToken: {RefreshToken}, ExpiresOn: {ExpiresOn}, JwtValidTo: {JwtValidTo}", refreshToken, refreshTokenFromDb?.ExpiresOn, jwtToken.ValidTo);
                return result.Failure("Invalid or expired refresh token or JWT token still valid", 401);
            }

            var AccessTokenExpiry = configuration["Jwt:AccessTokenExpiry"];
            logger.LogInformation("Access token expiry from configuration: {AccessTokenExpiry}", AccessTokenExpiry);
            var jwtKeyString = configuration["Jwt:Key"]
                                    ?? throw new InvalidOperationException("Jwt:Key configuration is missing for refresh token generation");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKeyString));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);


            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: jwtToken.Claims,
                expires: DateTime.UtcNow.AddMinutes(int.TryParse(AccessTokenExpiry, out var accessTokenExpiry) ? accessTokenExpiry : throw new ArgumentNullException(nameof(AccessTokenExpiry), "Access Token Expiry is not set")),
                signingCredentials: credentials
        );

            var newRefreshToken = await refreshTokenRepository.AddNewRefreshTokenAsync(refreshTokenFromDb.UserId);
            logger.LogInformation("Generated new refresh token for user ID: {UserId}", refreshTokenFromDb.UserId);

            return result.Success(new RefreshTokenResponseDto
            {
                RefreshToken = newRefreshToken,
                AccessToken = handler.WriteToken(token)
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while creating the token for access token: {AccessToken}", accessToken);
            return result.Failure("Invalid Access Token", 401);
        }
    }
}