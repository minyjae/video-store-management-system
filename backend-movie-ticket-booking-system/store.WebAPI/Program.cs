using store.Application;
using store.Infrastructure;
using store.WebAPI.Middleware;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using store.Infrastructure.Data;
using store.Domain.Entities;
using store.Domain.Enums;
using store.Application.Interfaces;

DotNetEnv.Env.Load();

var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException("JWT_SECRET is not set in .env");

var builder = WebApplication.CreateBuilder(args);

// ➊ ลงทะเบียน Services แยกเป็นกลุ่มตาม Layer
builder.Services.AddApplication();                        // Application Layer
builder.Services.AddInfrastructure(); // Infrastructure Layer

// แปลง Enum ให้เป็น string ส่งมาแทน
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            RoleClaimType = "role"  // ← บอก ASP.NET ว่า role claim ชื่อ "role" ใน JWT
        };
    });

var app = builder.Build();

// ➋ Apply EF Core migrations + Seed Admin อัตโนมัติตอน startup
var adminUsername = Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "admin";
var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD")
    ?? throw new InvalidOperationException("ADMIN_PASSWORD is not set in .env");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any(u => u.Role == UserRole.Admin))
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var admin = User.Register(adminUsername, hasher.Hash(adminPassword), UserRole.Admin);
        db.Users.Add(admin);
        db.SaveChanges();
    }
}

// ➌ เพิ่ม Global Exception Middleware (ต้องอยู่ก่อน Middleware อื่น)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();