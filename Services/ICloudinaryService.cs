using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CabinetMedicalWeb.Services
{
    public interface ICloudinaryService
    {
        // Returns a tuple: (Url, PublicId)
        Task<(string Url, string PublicId)> UploadScanAsync(IFormFile file, string folderPath);
    }
}