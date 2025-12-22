using MediatR;

namespace Application.Features.MessageFeatures.Commands;

public record ReactToMessageCommand(Guid MessageId, Guid UserId, string ReactionType) : IRequest<string>;

