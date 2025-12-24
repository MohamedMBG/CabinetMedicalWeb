using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace CabinetMedicalWeb.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var section = config.GetSection("Cloudinary");
            var account = new Account(
                section["CloudName"],
                section["ApiKey"],
                section["ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        public async Task<(string Url, string PublicId)> UploadScanAsync(IFormFile file, string folderPath)
        {
            if (file == null || file.Length == 0) return (null, null);

            using var stream = file.OpenReadStream();
            
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folderPath, // e.g., "patients/12/lab-results/55"
                
                // OPTIONAL TRANSFORMATIONS (Optimization)
                Transformation = new Transformation()
                    .Quality("auto")  // Auto-compress
                    .FetchFormat("auto") // Convert to WebP if browser supports it
                    .Width(1200).Crop("limit") // Resize if huge
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
            {
                throw new Exception($"Cloudinary Error: {uploadResult.Error.Message}");
            }

            return (uploadResult.SecureUrl.ToString(), uploadResult.PublicId);
        }
    }
}