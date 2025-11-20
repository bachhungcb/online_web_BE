using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.FriendFeatures.Commands;

public record AcceptFriendRequestCommand(
    Guid FriendRequestId,
    Guid ReceiverId) : IRequest<bool>;

public class AcceptFriendRequestCommandHandler : IRequestHandler<AcceptFriendRequestCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public AcceptFriendRequestCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(AcceptFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var fr = await _unitOfWork.FriendRequestRepository.GetById(request.FriendRequestId);

        if (fr == null)
        {
            throw new Exception("Can NOT find Friend Request");
        }

        // ▼▼▼ BẢO MẬT IDOR ▼▼▼
        if (fr.ReceiverId != request.ReceiverId)
        {
            throw new Exception("You do NOT have authority to do this");
        }

        // Kiểm tra xem đã là bạn bè chưa (đề phòng race condition)
        if (await _unitOfWork.FriendRepository.IsFriendAsync(fr.SenderId, fr.ReceiverId))
        {
            // Nếu đã là bạn, chỉ cần dọn dẹp request
            _unitOfWork.FriendRequestRepository.Remove(fr);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new Exception("Already friend. Aborting request");
        }

        // --- BẮT ĐẦU GIAO DỊCH (Unit of Work) ---

        // 1. Tạo bản ghi Friend
        var newFriend = new Friend
        {
            UserA = fr.SenderId,
            UserB = fr.ReceiverId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        _unitOfWork.FriendRepository.Add(newFriend);

        // 2. Xóa bản ghi FriendRequest
        _unitOfWork.FriendRequestRepository.Remove(fr);

        // 3. Lưu tất cả thay đổi
        var result = await _unitOfWork.SaveChangesAsync(cancellationToken);

        // --- KẾT THÚC GIAO DỊCH ---

        return result > 0; // Trả về true nếu có bản ghi bị thay đổi
    }
}