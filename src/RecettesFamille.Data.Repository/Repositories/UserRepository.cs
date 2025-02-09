using Microsoft.AspNetCore.Identity;
using RecettesFamille.Data.Repository.IRepositories;

namespace RecettesFamille.Data.Repository.Repositories;

public class UserRepository(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : IUserRepository
{
    public async Task AssignRoleAsync(string userEmail, string role)
    {
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user != null)
        {
            var roleExist = await roleManager.RoleExistsAsync(role);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }

    public async Task RemoveRoleAsync(string userEmail, string role)
    {
        var user = await userManager.FindByEmailAsync(userEmail);
        if (user != null && await userManager.IsInRoleAsync(user, role))
        {
            await userManager.RemoveFromRoleAsync(user, role);
        }
    }

    public async Task<List<(ApplicationUser User, List<string> Roles)>> GetAllUsersWithRolesAsync()
    {
        var users = userManager.Users.ToList();
        var usersWithRoles = new List<(ApplicationUser, List<string>)>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            usersWithRoles.Add((user, roles.ToList()));
        }

        return usersWithRoles;
    }
}
