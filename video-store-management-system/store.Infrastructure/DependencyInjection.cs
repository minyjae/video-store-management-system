using store.Domain.Interfaces;
using store.Infrastructure.Data;
using store.Infrastructure.Repositories;
using store.Application.Interfaces;
using store.Application.Services;

namespace store.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<MongoDbContext>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        return services;
    }
}