using Microsoft.EntityFrameworkCore;
using RecettesFamille.Data;
using RecettesFamille.Managers.AiGenerators;

namespace RecettesFamille
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddManagers(this IServiceCollection services)
        {
            services.AddKeyedScoped<IIaManagerBase, OpenAiManager>("OpenAi");
            services.AddKeyedScoped<IIaManagerBase, DeepSeekManager>("DeepSeek");

            return services;
        }

        public static IServiceCollection AddCustomDbContext(this IServiceCollection services)
        {
            services.AddDbContextFactory<ApplicationDbContext>(options =>
            {
                var postgresCs = "Host=recettes.atelier-cremazie.com;Port=5442;Database=recettesfamilledb;Username=pguser;Password=PGUserPwd;Pooling=true";
                options.UseNpgsql(postgresCs);
            }, ServiceLifetime.Scoped);

            return services;
        }
    }
}
