using MediatR;
using Microsoft.Extensions.Configuration;
using StreamChat.Clients;
using StreamChat.Models;

namespace Application.Features.VideoCallFeatures.Command;

public record UpsertStreamUserCommand(Guid UserId, string UserName, string Role = "user") : IRequest<bool>;

public class UpsertStreamUserHandler : IRequestHandler<UpsertStreamUserCommand, bool>
{
    private readonly IConfiguration _configuration;

    public UpsertStreamUserHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> Handle(UpsertStreamUserCommand request, CancellationToken cancellationToken)
    {
        string apiKey = _configuration["VideoCall:ApiKey"]!;
        string apiSecret = _configuration["VideoCall:Secret"]!;

        var factory = new StreamClientFactory(apiKey, apiSecret);
        var userClient = factory.GetUserClient();
        
        // Mapping dữ liệu sang User Object của Stream SDK
        var streamUser = new UserRequest()
        {
            Id = request.UserId.ToString(),
            Name = request.UserName,
            Role = request.Role, // "User"
        };

        // Thực hiện Upsert (Insert hoặc Update nếu đã tồn tại)
        // Hành động này đảm bảo tính toàn vẹn dữ liệu (Data Integrity)
        var user = await userClient.UpsertAsync(streamUser);

        return user != null;
    }
}