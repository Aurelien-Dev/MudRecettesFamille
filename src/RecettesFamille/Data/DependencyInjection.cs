using Microsoft.EntityFrameworkCore;

namespace RecettesFamille.Data
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCustomDbContext(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var postgresCs = "Host=172.26.0.29;Port=5432;Database=recettesfamilledb;Username=pguser;Password=PGUserPwd";
                options.UseNpgsql(postgresCs);
            }, ServiceLifetime.Transient);

            return services;
        }
    }
}
