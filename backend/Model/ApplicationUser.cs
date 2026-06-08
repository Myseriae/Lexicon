using Microsoft.AspNetCore.Identity;

namespace Lexicon.Model;

public class ApplicationUser : IdentityUser
{
    public bool IsDeleted { get; set; }
}
