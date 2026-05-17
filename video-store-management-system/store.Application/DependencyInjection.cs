namespace store.Application;

using Microsoft.Extensions.DependencyInjection;
using FluentValidation; // <-- ต้องใส่ using ตัวนี้
using store.Application.Interfaces;
using store.Application.Services;
using store.Application.Validators; // <-- และ using namespace ของ Validator

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ลงทะเบียน Services
        services.AddScoped<IMovieService, MovieService>();
        services.AddScoped<IAuthService, AuthService>();

        // ตอนนี้ Compiler จะรู้จัก Method นี้แล้ว
        services.AddValidatorsFromAssemblyContaining<CreateMovieValidator>();

        return services;
    }
}