using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BlazorCMS.Infrastructure.Storage
{
    public class FileStorageService
    {
        private readonly string _storagePath;

        public FileStorageService()
        {
            _storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "uploads");
            Directory.CreateDirectory(_storagePath);
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(_storagePath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }
    }
}
