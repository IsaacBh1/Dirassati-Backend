namespace Dirassati_Backend.Data.Models;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public DateTime ExpiresOn { get; set; }
    
    //Navigation property
    public AppUser User { get; set; } = null!;
        
}