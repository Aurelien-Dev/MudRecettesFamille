using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data;
using RecettesFamille.Managers;

namespace RecettesFamille
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddManagers(this IServiceCollection services)
        {
            services.AddScoped<GptRecipeConverterManager>();

            return services;
        }

        public static IServiceCollection AddCustomDbContext(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var postgresCs = "Host=recettes.atelier-cremazie.com;Port=5442;Database=recettesfamilledb;Username=pguser;Password=PGUserPwd";
                options.UseNpgsql(postgresCs);
            }, ServiceLifetime.Transient);

            return services;
        }
    }
}
