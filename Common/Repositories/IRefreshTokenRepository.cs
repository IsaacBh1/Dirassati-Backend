using Dirassati_Backend.Data.Models;

namespace Dirassati_Backend.Common.Repositories;

public interface IRefreshTokenRepository
{
    Task<string> AddNewRefreshTokenAsync(string userId);
    Task UpdateAsync(RefreshToken refreshToken);
    Task DeleteAsync(string token);
    Task<bool> ExistsAsync(string token);
    
    Task<RefreshToken?> FindByTokenAsync(string token);
    Task<bool> RevokeRefreshTokenAsync(string token);
    Task<bool>RevokeAllRefreshTokensAsync();
}