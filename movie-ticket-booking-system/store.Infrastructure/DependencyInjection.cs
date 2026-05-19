// store.Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using store.Domain.Interfaces;
using store.Infrastructure.Data;
using store.Infrastructure.Repositories;
using store.Application.Services;
using store.Application.Interfaces;

namespace store.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // PostgreSQL
        // ใช้ timestamp without time zone (เก็บ UTC+7 ตรงๆ ไม่ทำ timezone conversion)
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")
            ?? throw new InvalidOperationException("POSTGRES_CONNECTION is not set.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<ILedgerRepository, LedgerRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IShowtimeRepository, ShowtimeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Auth Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(
                Environment.GetEnvironmentVariable("REDIS_CONNECTION")!));

        return services;
    }
}