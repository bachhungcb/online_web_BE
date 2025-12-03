using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Service;

public interface IFileService
{
    // Upload file and return URL
    Task<string> UploadFileAsync(IFormFile file, string folderName);
    
    // Delete file
    Task DeleteFileAsync(string fileUrl);
}