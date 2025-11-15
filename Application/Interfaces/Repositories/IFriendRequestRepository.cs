using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IFriendRequestRepository : IGenericRepository<FriendRequest>
{
    // Lấy các request đã nhận
    Task<IEnumerable<FriendRequest>> GetReceivedRequestsAsync(Guid receiverId);
    // Lấy các request đã gửi
    Task<IEnumerable<FriendRequest>> GetSentRequestsAsync(Guid senderId);
    // Kiểm tra request đã tồn tại chưa
    Task<bool> RequestExistsAsync(Guid senderId, Guid receiverId);
}