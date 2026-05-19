using System.Net;
using System.Text.Json;
using FluentValidation;

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
    // 1. ตั้งค่า Default กรณีเป็น Error ที่เราไม่ได้ดักไว้ (500)
    var statusCode = HttpStatusCode.InternalServerError;
    object responseMessage = "เกิดข้อผิดพลาดภายในระบบ กรุณาลองใหม่อีกครั้ง";

    // 2. คัดแยกประเภท Exception
    switch (exception)
    {
        // กลุ่ม Validation ของ FluentValidation (ส่ง error กลับไปเป็น list)
        case ValidationException validationEx:
            statusCode = HttpStatusCode.BadRequest;
            responseMessage = validationEx.Errors.Select(e => e.ErrorMessage); 
            break;

        // กลุ่มข้อมูลผิดพลาด
        case ArgumentException:
            statusCode = HttpStatusCode.BadRequest;
            responseMessage = exception.Message;
            break;

        // กลุ่มหาไม่เจอ
        case KeyNotFoundException:
            statusCode = HttpStatusCode.NotFound;
            responseMessage = exception.Message;
            break;

        // กลุ่ม Business Logic (เช่น เงินไม่พอ, ที่นั่งเต็ม)
        case InvalidOperationException:
            statusCode = HttpStatusCode.BadRequest;
            responseMessage = exception.Message;
            break;

        // กลุ่ม Concurrency (ถ้าคุณสร้าง Custom Exception ไว้ดักคนแย่งกันซื้อตั๋ว)
        // case ConcurrencyException:
        //     statusCode = HttpStatusCode.Conflict;
        //     responseMessage = exception.Message;
        //     break;

        // กลุ่ม Security
        case UnauthorizedAccessException:
            statusCode = HttpStatusCode.Unauthorized;
            responseMessage = "คุณไม่มีสิทธิ์เข้าถึงข้อมูลนี้ หรือ Token อาจจะหมดอายุ";
            break;
    }

    // 3. ประกอบร่าง HTTP Response
    context.Response.StatusCode = (int)statusCode;
    context.Response.ContentType = "application/json";

    // 4. ส่งกลับเป็น JSON
    var response = new { error = responseMessage, statusCode = (int)statusCode };
    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
}
}