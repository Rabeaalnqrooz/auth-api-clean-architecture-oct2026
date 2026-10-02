using AuthApi.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AuthApi.API.Middleware;

/// <summary>
/// يلتقط كل الاستثناءات غير المعالجة.
/// AppException → رسالة للعمل + StatusCode
/// غير ذلك → رسالة عامة في Production (ملاحظة 6)
/// </summary>
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // نسجّل دائماً بالتفاصيل للمطور
            _logger.LogError(ex, "Unhandled exception");
            await WriteProblemAsync(context, ex);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        int status;
        string title;
        string detail;

        switch (exception)
        {
            case AppException appEx:
                status = appEx.StatusCode;
                title = TitleFor(status);
                detail = appEx.Message; // رسائلنا المقصودة — آمنة
                break;

            case UnauthorizedAccessException:
                status = StatusCodes.Status401Unauthorized;
                title = "Unauthorized";
                detail = string.IsNullOrWhiteSpace(exception.Message)
                    ? "غير مصرح"
                    : exception.Message;
                break;

            default:
                status = StatusCodes.Status500InternalServerError;
                title = "Internal Server Error";
                // لا نسرّب SqlException للعميل في الإنتاج
                detail = _env.IsDevelopment()
                    ? exception.Message
                    : "حدث خطأ غير متوقع. حاول لاحقاً";
                break;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["success"] = false;

        if (_env.IsDevelopment())
        {
            problem.Extensions["exceptionType"] = exception.GetType().Name;
            problem.Extensions["stackTrace"] = exception.StackTrace;
        }

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static string TitleFor(int status) => status switch
    {
        400 => "Bad Request",
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not Found",
        _ => "Error"
    };
}