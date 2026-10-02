namespace AuthApi.Application.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message)
        : base(message, 401)
    {
    }
}
//Login فاشل، جلسة منتهية