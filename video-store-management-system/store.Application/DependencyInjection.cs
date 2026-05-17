namespace Store.Application;

using Microsoft.Extensions.DependencyInjection;
using FluentValidation; // <-- ต้องใส่ using ตัวนี้
using Store.Application.Interfaces;
using Store.Application.Services;
using Store.Application.Validators; // <-- และ using namespace ของ Validator
 
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ลงทะเบียน Services
        services.AddScoped<IMovieService, MovieService>();

        // ตอนนี้ Compiler จะรู้จัก Method นี้แล้ว
        services.AddValidatorsFromAssemblyContaining<CreateMovieValidator>();

        return services;
    }
}