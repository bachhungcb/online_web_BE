using Application.Interfaces.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Tools.Services;

public class LocalFileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalFileService(IWebHostEnvironment environment, IHttpContextAccessor httpContextAccessor)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folderName)
    {
        if (file == null || file.Length == 0)
            throw new Exception("File is Empty");


        // 1. Validate: Using magic numbers
        var allowExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".docx", ".txt", "doc", ".xlsx" };
        var extension = Path.GetExtension(file.FileName).ToLower();

        if (!allowExtensions.Contains(extension))
            throw new Exception("Invalid file type");
        
        // 2. Randomize file name
        // prevent path traversal 
        var fileName = $"{Guid.NewGuid()}{extension}";
        
        // Create save path
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folderName);
        if (!Directory.Exists(uploadsFolder)) // create new Dir if dir is not yet exist
            Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, fileName);
        
        // 3. Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        // 4. Trả về URL để truy cập
        var request = _httpContextAccessor.HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        
        // Output format: https://localhost:5000/uploads/avatars/guid.jpg
        return $"{baseUrl}/uploads/{folderName}/{fileName}";
    }

    public Task DeleteFileAsync(string fileUrl)
    {
        throw new NotImplementedException();
    }
}