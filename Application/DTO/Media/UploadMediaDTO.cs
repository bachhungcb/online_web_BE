using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Application.DTO.Media;

public class UploadMediaDto
{
    [Required]
    public IFormFile File { get; set; }
}