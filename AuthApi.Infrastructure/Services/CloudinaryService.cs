using AuthApi.Application.Interfaces;
using AuthApi.Application.Options;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace AuthApi.Infrastructure.Services;

public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> options)
    {
        var s = options.Value;
        _cloudinary = new Cloudinary(new Account(s.CloudName, s.ApiKey, s.ApiSecret));
    }

    public async Task<(string Url, string PublicId)> UploadImageAsync(Stream fileStream, string fileName)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = "users/profiles",
            Transformation = new Transformation()
                .Width(500).Height(500).Crop("fill").Gravity("face")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
            throw new InvalidOperationException(result.Error.Message);

        return (result.SecureUrl.ToString(), result.PublicId);
    }

    public async Task DeleteImageAsync(string? publicId)
    {
        if (string.IsNullOrWhiteSpace(publicId) || publicId.StartsWith("defaults/"))
            return;

        await _cloudinary.DestroyAsync(new DeletionParams(publicId));
    }
}