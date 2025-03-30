using Microsoft.AspNetCore.Identity;
using Dirassati_Backend.Data.Models;
public class AppUser : IdentityUser
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateOnly BirthDate { get; set; }
    public int? AdresseId { get; set; }
    public virtual Address? Address { get; set; } = new();

}