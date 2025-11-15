using Application.Interfaces.Repositories;
using DataAccess.EFCore.Context;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.EFCore.Persistence.Repositories;

public class FriendRequestRepository : GenericRepository<FriendRequest>, IFriendRequestRepository
{
    public FriendRequestRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    // Lấy các request đã nhận
    public Task<IEnumerable<FriendRequest>> GetReceivedRequestsAsync(Guid receiverId)
    {
        throw new NotImplementedException();
    }

    // Lấy các request đã gửi
    public Task<IEnumerable<FriendRequest>> GetSentRequestsAsync(Guid senderId)
    {
        throw new NotImplementedException();
    }

    // Kiểm tra request đã tồn tại chưa
    public async Task<bool> RequestExistsAsync(Guid senderId, Guid receiverId)
    {
        return await _dbContext.FriendRequests
            .AsNoTracking()
            .AnyAsync(f => 
                (f.SenderId == senderId && f.ReceiverId == receiverId) ||
                (f.SenderId == receiverId && f.ReceiverId == senderId)
            );
    }
}