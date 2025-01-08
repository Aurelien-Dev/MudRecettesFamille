using Microsoft.EntityFrameworkCore;

namespace RecettesFamille.Data
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCustomDbContext(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var postgresCs = "Host=localhost;Port=5432;Database=recettesfamilledb2;Username=pguser;Password=PGUserPwd";
                options.UseNpgsql(postgresCs);
            }, ServiceLifetime.Transient);

            //Apply DB Migration
            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
            dbContext.SaveChanges();


            return services;
        }
    }
}
