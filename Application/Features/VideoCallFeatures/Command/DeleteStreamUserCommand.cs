using MediatR;
using Microsoft.Extensions.Configuration;
using StreamChat.Clients;

namespace Application.Features.VideoCallFeatures.Command;

public record DeleteStreamUserCommand(Guid UserId) : IRequest<bool>;

public class DeleteStreamUserHandler : IRequestHandler<DeleteStreamUserCommand, bool>
{
    private readonly IConfiguration _configuration;

    public DeleteStreamUserHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> Handle(DeleteStreamUserCommand request, CancellationToken cancellationToken)
    {
        string apiKey = _configuration["VideoCallApi:ApiKey"]!;
        string apiSecret = _configuration["VideoCallApi:Secret"]!;

        var factory = new StreamClientFactory(apiKey, apiSecret);
        var userClient = factory.GetUserClient();

        // Xóa User khỏi Stream
        await userClient.DeleteAsync(request.UserId.ToString());

        return true;
    }
}