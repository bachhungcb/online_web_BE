using Application.Interfaces.Service;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.MediaFeatures.Commands;

public record UploadMediaCommand(IFormFile File, string Folder) : IRequest <string>;

public class UploadMediaCommandHandler : IRequestHandler<UploadMediaCommand, string>
{
    private readonly IFileService _fileService;

    public UploadMediaCommandHandler(IFileService fileService)
    {
        _fileService = fileService;
    }
    
    public async Task<string> Handle(UploadMediaCommand request, CancellationToken cancellationToken)
    {
        return await _fileService.UploadFileAsync(request.File, request.Folder);
    }
}