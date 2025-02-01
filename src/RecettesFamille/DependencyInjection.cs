using RecettesFamille.Managers;
using RecettesFamille.Managers.AiGenerators;

namespace RecettesFamille
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddManagers(this IServiceCollection services)
        {
            services.AddKeyedScoped<IIaManagerBase, OpenAiManager>("OpenAi");
            services.AddKeyedScoped<IIaManagerBase, DeepSeekManager>("DeepSeek");

            services.AddScoped<ErrorManager>();

            return services;
        }
    }
}
