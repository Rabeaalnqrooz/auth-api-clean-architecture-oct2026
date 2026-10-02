namespace AuthApi.Application.Interfaces;

public interface ICloudinaryService
{
    Task<(string Url, string PublicId)> UploadImageAsync(Stream fileStream, string fileName);
    Task DeleteImageAsync(string? publicId);
}