using System.Net;
using System.Text.Json;

namespace store.WebAPI.Middleware;

// Middleware จัดการ Exception ทั้งหมดในที่เดียว — ไม่ต้อง try-catch ทุก controller
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);   // เรียก middleware ถัดไป
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "เกิด exception ที่ไม่ได้จัดการ");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            ArgumentException    => (HttpStatusCode.BadRequest, exception.Message),
            _                    => (HttpStatusCode.InternalServerError, "เกิดข้อผิดพลาด กรุณาลองใหม่")
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new { error = message, statusCode = (int)statusCode };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}