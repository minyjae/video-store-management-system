namespace store.Application;

using Microsoft.Extensions.DependencyInjection;
using FluentValidation; 
using store.Application.Interfaces;
using store.Application.Services;
using store.Application.Validators; 

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    { // ย่อหน้าให้อยู่ตรงแนวนี้จะอ่านง่ายขึ้นครับ
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IShowtimeService, ShowtimeService>();
        services.AddScoped<ITicketBookingService, TicketBookingService>();
        services.AddScoped<ISeatService, SeatService>();
        services.AddScoped<IWalletService, WalletService>(); 

        services.AddValidatorsFromAssemblyContaining<CreateMovieValidator>();

        return services;
    }
}