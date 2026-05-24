using Microsoft.AspNetCore.Identity;

namespace Custom_Tournaments_Infrastructure
{
    public class AppUser : IdentityUser
    {
        public string? Avatarurl { get; set; }
    }
}