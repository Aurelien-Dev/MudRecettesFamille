using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using RecettesFamille.Data.Repository.IRepositories;
using RecettesFamille.Data.Repository.Mappers;
using RecettesFamille.Data.Repository.Repositories;

namespace RecettesFamille.Data.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepository(this IServiceCollection services)
    {
        //Repositories
        services.AddScoped<IRecetteRepository, RecetteRepository>();
        services.AddScoped<IAiRepository, AiRepository>();
        services.AddScoped<IStatisticRepository, StatisticRepository>();
        services.AddScoped<ITechnicalDbRepository, TechnicalDbRepository>();

        //AutoMapper
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        IMapper mapper = mapperConfig.CreateMapper();
        services.AddSingleton(mapper);

        return services;
    }
}