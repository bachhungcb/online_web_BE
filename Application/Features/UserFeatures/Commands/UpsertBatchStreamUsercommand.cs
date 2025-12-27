using Application.Features.UserFeatures.Queries;
using MediatR;
using Microsoft.Extensions.Configuration;
using StreamChat.Clients;
using StreamChat.Models;

namespace Application.Features.UserFeatures.Commands;

public record UpsertBatchStreamUsercommand : IRequest<bool>;

public class UpsertBatchStreamUsercommandHandler : IRequestHandler<UpsertBatchStreamUsercommand, bool>
{
    private readonly IConfiguration _configuration;
    private readonly IMediator _mediator;

    public UpsertBatchStreamUsercommandHandler(
        IConfiguration configuration,
        IMediator mediator)
    {
        _configuration = configuration;
        _mediator = mediator;
    }

    public async Task<bool> Handle(UpsertBatchStreamUsercommand request, CancellationToken cancellationToken)
    {
        try
        {
            string apiKey = _configuration["VideoCall:ApiKey"]!;
            string apiSecret = _configuration["VideoCall:ApiSecret"]!;

            var factory = new StreamClientFactory(apiKey, apiSecret);
            var userClient = factory.GetUserClient();

            var userList = await _mediator.Send(new GetAllUserQuery(1, 50), cancellationToken);

            foreach (var user in userList)
            {
                var streamUser = new UserRequest()
                {
                    Id = user.Id.ToString(),
                    Name = user.UserName,
                    Role = "user"
                };

                await userClient.UpsertAsync(streamUser);
            }

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}