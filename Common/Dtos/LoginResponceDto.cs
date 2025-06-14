namespace Dirassati_Backend.Common.Dtos;

public class RefreshTokenResponseDto
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
   
}