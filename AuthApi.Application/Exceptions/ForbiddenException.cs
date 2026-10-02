namespace AuthApi.Application.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message)
        : base(message, 403)
    {
    }
}
//ليس مالك المحل / ليس أدمن