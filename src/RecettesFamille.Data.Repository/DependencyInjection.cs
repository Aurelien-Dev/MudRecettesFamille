using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Data.Repository.Mappers;
using RecettesFamille.Data.Repository.Repositories;

namespace RecettesFamille.Data.Repository;

public static class DependencyInjection
{
    public static void AddRepository(this IServiceCollection services)
    {
        //Repositories
        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<IAiRepository, AiRepository>();
        services.AddScoped<IStatisticRepository, StatisticRepository>();
        services.AddScoped<ITechnicalDbRepository, TechnicalDbRepository>();
        services.AddScoped<ITagRepository, TagRepository>();

        //AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        var mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);
    }
}