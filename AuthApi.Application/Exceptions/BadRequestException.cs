namespace AuthApi.Application.Exceptions;

public class BadRequestException : AppException
{
    public BadRequestException(string message)
        : base(message, 400)
    {
    }
}
//OTP غلط، بيانات غير صالحة