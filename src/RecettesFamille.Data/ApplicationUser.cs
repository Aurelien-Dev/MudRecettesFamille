using Microsoft.AspNetCore.Identity;
using RecettesFamille.Data.EntityModel;

namespace RecettesFamille.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string AccountName { get; set; } = string.Empty;

    public ICollection<RecipeEntity> Favorites { get; set; } = null!;
}
