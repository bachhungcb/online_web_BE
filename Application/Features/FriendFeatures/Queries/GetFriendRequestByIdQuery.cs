using Application.DTO.Friends;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.FriendFeatures.Queries;

public record GetFriendRequestByIdQuery(
    Guid RequestId,
    Guid CurrentUserId) : IRequest<FriendRequestDto>
{
    public class GetFriendRequestByIdQueryHandler
        : IRequestHandler<GetFriendRequestByIdQuery, FriendRequestDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetFriendRequestByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<FriendRequestDto> Handle(GetFriendRequestByIdQuery request,
            CancellationToken cancellationToken)
        {
            var fr = await _unitOfWork.FriendRequestRepository.GetById(request.RequestId);

            if (fr == null)
            {
                throw new Exception("Cannot find Friend Request");
            }

            if (fr.SenderId != request.CurrentUserId && fr.ReceiverId != request.CurrentUserId)
            {
                throw new Exception("Cannot find Friend Request");
            }

            var sender = await _unitOfWork.UserRepository.GetById(fr.SenderId);
            var receiver = await _unitOfWork.UserRepository.GetById(fr.ReceiverId);

            return new FriendRequestDto
            {
                Id = fr.Id,
                SenderId = fr.SenderId,
                SenderName = sender?.UserName ?? "N/A",
                ReceiverId = fr.ReceiverId,
                ReceiverName = receiver?.UserName ?? "N/A",
                Message = fr.Message,
                CreatedAt = fr.CreatedAt
            };
        }
    }
}