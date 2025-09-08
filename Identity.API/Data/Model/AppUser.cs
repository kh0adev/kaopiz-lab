using Microsoft.AspNetCore.Identity;

namespace Identity.API.Data.Model;

public class AppUser: IdentityUser
{
    public DateTime DateOfBirth { get; set; }
    public bool Gender { get; set; }
}