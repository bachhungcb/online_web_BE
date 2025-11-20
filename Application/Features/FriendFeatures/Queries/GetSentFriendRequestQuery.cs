using Application.DTO.Friends;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.FriendFeatures.Queries;

public record GetSentFriendRequestQuery(Guid UserId) : IRequest<IEnumerable<FriendRequestDto>>
{
    public class
        GetSentFriendRequestQueryHandler : IRequestHandler<GetSentFriendRequestQuery, IEnumerable<FriendRequestDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSentFriendRequestQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<FriendRequestDto>> Handle(
            GetSentFriendRequestQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Gọi Repository lấy dữ liệu
            var requests = await _unitOfWork.FriendRequestRepository
                .GetSentRequestsAsync(request.UserId);

            // 2. Map từ Entity sang DTO
            // (Lưu ý: FriendRequestDto của bạn đã có trong project)
            return requests.Select(r => new FriendRequestDto
            {
                Id = r.Id,
                SenderId = r.SenderId,
                // Sử dụng toán tử null-conditional (?.) và null-coalescing (??) để an toàn
                SenderName = r.Sender?.FullName ?? "Unknown", 
                ReceiverId = r.ReceiverId,
                ReceiverName = "Me", // Vì đây là list đã nhận, receiver chính là mình
                Message = r.Message,
                CreatedAt = r.CreatedAt
            });
        }
    }
}