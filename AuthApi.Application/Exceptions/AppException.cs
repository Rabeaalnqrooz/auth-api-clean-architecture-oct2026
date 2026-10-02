namespace AuthApi.Application.Exceptions;

/// <summary>
/// الخطأ الأساسي لأخطاء العمل المتوقعة.
/// ExceptionMiddleware يعتمد عليها لإرجاع StatusCode ورسالة آمنة.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400)
        : base(message)
    {
        StatusCode = statusCode;
    }
}