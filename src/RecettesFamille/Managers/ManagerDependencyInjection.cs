namespace RecettesFamille.Managers;

public static class ManagerDependencyInjection
{
    public static IServiceCollection AddManagers(this IServiceCollection services)
    {
        services.AddScoped<GptRecipeConverterManager>();

        return services;
    }
}