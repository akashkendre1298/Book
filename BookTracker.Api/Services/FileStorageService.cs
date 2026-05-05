using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookTracker.Api.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly string _uploadFolder;

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
        _uploadFolder = Path.Combine(_env.ContentRootPath, "uploads");
        if (!Directory.Exists(_uploadFolder)) Directory.CreateDirectory(_uploadFolder);
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subFolder, string[] allowedExtensions, long maxSizeBytes)
    {
        if (file == null || file.Length == 0) throw new ArgumentException("File is empty");
        if (file.Length > maxSizeBytes) throw new ArgumentException("File size exceeds limit");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension)) throw new ArgumentException("Invalid file extension");

        // Validate Magic Bytes
        if (extension == ".pdf" && !IsPdf(file)) throw new ArgumentException("Invalid PDF file signature");
        if ((extension == ".jpg" || extension == ".jpeg" || extension == ".png") && !IsImage(file)) 
            throw new ArgumentException("Invalid image file signature");

        var folderPath = Path.Combine(_uploadFolder, subFolder);
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{subFolder}/{fileName}";
    }

    public bool DeleteFile(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return false;
        
        // Prevent path traversal
        var fileName = Path.GetFileName(relativePath);
        var subFolder = Path.GetDirectoryName(relativePath)?.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).LastOrDefault() ?? "";
        
        var fullPath = Path.Combine(_uploadFolder, subFolder, fileName);
        
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            return true;
        }
        return false;
    }

    public bool IsPdf(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new BinaryReader(stream);
        var bytes = reader.ReadBytes(4);
        // PDF magic bytes: %PDF (0x25 0x50 0x44 0x46)
        return bytes.Length == 4 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46;
    }

    public bool IsImage(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new BinaryReader(stream);
        var bytes = reader.ReadBytes(4);
        
        // JPEG: FF D8 FF
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return true;
        
        // PNG: 89 50 4E 47
        if (bytes.Length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47) return true;
        
        return false;
    }
}
