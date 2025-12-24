using MediatR;
using Microsoft.Extensions.Configuration;
using StreamChat.Clients;

namespace Application.Features.VideoCallFeatures.Command;

public record CreateVideoCallTokenCommand(Guid senderId):IRequest<string>;

public class CreateVideoCallTokenHander : IRequestHandler<CreateVideoCallTokenCommand, string>
{

    private readonly IConfiguration _configuration;

    public CreateVideoCallTokenHander(IConfiguration configuration)
    {

        _configuration = configuration;
    }
    
    public async Task<string> Handle(CreateVideoCallTokenCommand request, CancellationToken cancellationToken)
    {
        // Lấy config và kiểm tra null ngay lập tức (Fail Fast)
        string apiKey = _configuration["VideoCallApi:ApiKey"] 
                        ?? throw new InvalidOperationException("VideoCallApi:ApiKey is not configured.");
        string apiSecret = _configuration["VideoCallApi:Secret"] 
                           ?? throw new InvalidOperationException("VideoCallApi:Secret is not configured.");
        
        var factory = new StreamClientFactory(apiKey, apiSecret);
        
        var userClient = factory.GetUserClient();
        
        var token = userClient.CreateToken(request.senderId.ToString());
        
        return token;
    }
}