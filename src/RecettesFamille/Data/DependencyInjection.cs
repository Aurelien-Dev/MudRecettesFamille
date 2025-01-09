using Microsoft.EntityFrameworkCore;

namespace RecettesFamille.Data
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCustomDbContext(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var postgresCs = "Host=recettes.atelier-cremazie.com;Port=5442;Database=recettesfamilledb;Username=pguser;Password=PGUserPwd";
                options.UseNpgsql(postgresCs);
            }, ServiceLifetime.Transient);

            //Apply DB Migration
            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();


            return services;
        }
    }
}
