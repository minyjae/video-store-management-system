using store.Domain.Interfaces;
using store.Infrastructure.Data;
using store.Infrastructure.Repositories;

namespace store.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<MongoDbContext>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        return services;
    }
}