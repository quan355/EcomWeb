using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using EComWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.CodeAnalysis;
namespace EComWeb.Repositories
{
    public interface ICloudinaryService
    {
        Task<ImageUploadResult> UploadImageAsync(IFormFile file, int productId);
        Task<DeletionResult> DeleteImageAsync(string publicId);
    }

    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        public CloudinaryService(IConfiguration config)
        {
            var settings = config.GetSection("CloudinarySettings").Get<CloudinarySettings>();
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new Cloudinary(account);
        }

        public async Task<ImageUploadResult> UploadImageAsync(IFormFile file,int productId)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                PublicId = $"products/{productId}",
                Transformation = new Transformation().Width(500).Height(500).Crop("fill")
            };
            return await _cloudinary.UploadAsync(uploadParams);
        }
        public async Task<DeletionResult> DeleteImageAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}
