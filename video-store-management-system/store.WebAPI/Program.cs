using Store.Application;
using store.Infrastructure;
using Store.WebAPI.Middleware;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// ➊ ลงทะเบียน Services แยกเป็นกลุ่มตาม Layer
builder.Services.AddApplication();                        // Application Layer
builder.Services.AddInfrastructure(); // Infrastructure Layer

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ➋ เพิ่ม Global Exception Middleware (ต้องอยู่ก่อน Middleware อื่น)
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();