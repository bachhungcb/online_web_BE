using MediatR;
using Microsoft.Extensions.Configuration;
using StreamChat.Clients;

namespace Application.Features.VideoCallFeatures.Command;

public record CreateVideoCallTokenCommand(List<Guid> UserIds):IRequest<Dictionary<Guid,string>>;
public class CreateVideoCallTokenHandler : IRequestHandler<CreateVideoCallTokenCommand, Dictionary<Guid, string>>
{

    private readonly IConfiguration _configuration;

    public CreateVideoCallTokenHandler(IConfiguration configuration)
    {

        _configuration = configuration;
    }
    
    public async Task<Dictionary<Guid,string>> Handle(CreateVideoCallTokenCommand request, CancellationToken cancellationToken)
    {
        // Lấy config và kiểm tra null ngay lập tức (Fail Fast)
        string apiKey = _configuration["VideoCall:ApiKey"] 
                        ?? throw new InvalidOperationException("VideoCall:ApiKey is not configured.");
        string apiSecret = _configuration["VideoCall:ApiSecret"] 
                           ?? throw new InvalidOperationException("VideoCall:Secret is not configured.");
        
        var factory = new StreamClientFactory(apiKey, apiSecret);
        var userClient = factory.GetUserClient();
        
        var tokens = new Dictionary<Guid, string>();
        
        foreach (var userId in request.UserIds)
        {
            var token = userClient.CreateToken(userId.ToString());
            tokens.Add(userId, token);
        }
        
        return tokens;
    }
}