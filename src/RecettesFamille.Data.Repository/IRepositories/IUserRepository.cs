using Microsoft.AspNetCore.Identity;

namespace RecettesFamille.Data.Repository.IRepositories
{
    public interface IUserRepository
    {
        Task AssignRoleAsync(string userEmail, string role);
        Task<List<(ApplicationUser User, List<string> Roles)>> GetAllUsersWithRolesAsync();
        Task RemoveRoleAsync(string userEmail, string role);
    }
}