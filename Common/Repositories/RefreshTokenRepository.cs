using Dirassati_Backend.Common.Security;
using Dirassati_Backend.Data.Models;
using Dirassati_Backend.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Dirassati_Backend.Common.Repositories;

public class RefreshTokenRepository(AppDbContext context, TokenProvider provider):IRefreshTokenRepository
{
    public async Task<string> AddNewRefreshTokenAsync(string userId)
    {
        var currentToken = await context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == userId);
        var newToken = provider.GenerateRefreshToken();
        if (currentToken is null)
        {
            var refreshToken = new RefreshToken
            {
                Token = newToken,
                ExpiresOn = DateTime.UtcNow.AddDays(7),
                UserId = userId,
            };
            context.RefreshTokens.Add(refreshToken);     
        }
        else
        {
            currentToken.Token = newToken;
            currentToken.ExpiresOn = DateTime.UtcNow.AddDays(7);
            context.RefreshTokens.Update(currentToken);
        }
     
       
         await context.SaveChangesAsync();
         return newToken;
    }

    public Task UpdateAsync(RefreshToken refreshToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(string token)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(string token)
    {
        throw new NotImplementedException();
    }

    public async Task<RefreshToken?> FindByTokenAsync(string token)
    {
        return await  context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

   public async Task<bool> RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
        if (refreshToken == null)
            return false;
    
        context.RefreshTokens.Remove(refreshToken);
        await context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> RevokeAllRefreshTokensAsync()
    {
      var allTokens = await context.RefreshTokens.ToListAsync();
              if (allTokens.Count == 0)
                  return false;
              context.RefreshTokens.RemoveRange(allTokens);
              await context.SaveChangesAsync();
              return true;
    }
}