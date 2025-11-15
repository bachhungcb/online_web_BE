using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.FriendFeatures.Commands;

public record DeleteFriendRequestCommand(
    Guid FriendRequestId,
    Guid CurrentUserId) : IRequest<bool>;

public class DeleteFriendRequestCommandHandler
    : IRequestHandler<DeleteFriendRequestCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteFriendRequestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var fr = await _unitOfWork.FriendRequestRepository.GetById(request.FriendRequestId);

        if (fr == null)
        {
            throw new Exception("Can NOT find friend request");
        }

        // ▼▼▼ BẢO MẬT IDOR ▼▼▼
        if (fr.SenderId != request.CurrentUserId && fr.ReceiverId != request.CurrentUserId)
        {
            throw new Exception("You do NOT have authority to do this");
        }

        _unitOfWork.FriendRequestRepository.Remove(fr);
        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0;
    }
}