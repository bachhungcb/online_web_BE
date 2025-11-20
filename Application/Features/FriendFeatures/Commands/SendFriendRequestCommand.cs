using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.FriendFeatures.Commands;
/// <summary>
/// Dữ liệu đầu vào cho lệnh gửi yêu cầu kết bạn.
/// SenderId sẽ được lấy từ token (ở Controller)
/// </summary>
public record SendFriendRequestCommand(
    Guid SenderId,
    Guid ReceiverId,
    string? Message
    ) : IRequest<Guid>
{
    /// <summary>
    /// Trình xử lý (Handler) cho lệnh gửi yêu cầu.
    /// </summary>
    public class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SendFriendRequestCommandHandler(
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Guid> Handle(
            SendFriendRequestCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Validation: Không thể gửi cho chính mình
            if (request.SenderId == request.ReceiverId)
            {
                // Lỗi nghiệp vụ (Business Logic Error)
                throw new Exception("Bạn không thể tự kết bạn với chính mình.");
            }

            // 2. Validation: Kiểm tra người nhận có tồn tại không
            // (Chúng ta giả định SenderId tồn tại vì họ đã được xác thực)
            var receiverUser = await _unitOfWork.UserRepository.GetById(request.ReceiverId);
            if (receiverUser == null)
            {
                // Lỗi không tìm thấy (Not Found Error)
                throw new Exception($"Người dùng với ID {request.ReceiverId} không tồn tại.");
            }

            // 3. Validation: Kiểm tra đã là bạn bè chưa
            var isAlreadyFriends = await _unitOfWork.FriendRepository
                .IsFriendAsync(request.SenderId, request.ReceiverId);
        
            if (isAlreadyFriends)
            {
                throw new Exception("Bạn đã là bạn bè với người này.");
            }

            // 4. Validation: Kiểm tra yêu cầu đã tồn tại (theo cả 2 hướng)
            var requestExists = await _unitOfWork.FriendRequestRepository
                .RequestExistsAsync(request.SenderId, request.ReceiverId);
        
            if (requestExists)
            {
                throw new Exception("Một yêu cầu kết bạn đang chờ xử lý với người này.");
            }

            // 5. Logic chính: Tạo và Lưu
            var newRequest = new FriendRequest
            {
                SenderId = request.SenderId,
                ReceiverId = request.ReceiverId,
                Message = request.Message,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // Thêm vào Repository (chưa ghi vào CSDL)
            _unitOfWork.FriendRequestRepository.Add(newRequest);

            // Ghi tất cả thay đổi vào CSDL trong một giao dịch (transaction)
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 6. Trả về ID của thực thể mới
            return newRequest.Id;
        }
    }
}