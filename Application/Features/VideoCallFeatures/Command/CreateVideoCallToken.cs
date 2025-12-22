using MediatR;
using Microsoft.Extensions.Configuration;
using StreamChat.Clients;

namespace Application.Features.VideoCallFeatures.Command;

// public record CreateVideoCallToken(Guid senderId):IRequest<string>;
//
// public class CreateVideoCallTokenHander : IRequestHandler<CreateVideoCallToken, string>
// {
//     private readonly IMediator _mediator;
//     private readonly IConfiguration _configuration;
//
//     private CreateVideoCallTokenHander(IMediator mediator, IConfiguration configuration)
//     {
//         _mediator = mediator;
//         _configuration = configuration;
//     }
//     
//     public async Task<string> Handle(CreateVideoCallToken request, CancellationToken cancellationToken)
//     {
//         var factory = new StreamClientFactory("{{ api_key }}", "{{ api_secret }}");
//         var userClient = factory.GetUserClient();
//         var token = userClient.CreateToken("john");
//         throw new NotImplementedException();
//     }
// }