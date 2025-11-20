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
    public async Task<IEnumerable<FriendRequest>> GetReceivedRequestsAsync(Guid receiverId)
    {
        return await _dbContext.FriendRequests
            .AsNoTracking()
            .Where(fr => fr.ReceiverId == receiverId) // Lọc theo người nhận
            .Include(fr => fr.Sender)                 // Kèm thông tin người gửi (để lấy tên/avatar)
            .OrderByDescending(fr => fr.CreatedAt)    // Mới nhất lên đầu
            .ToListAsync();
    }

    // Lấy các request đã gửi
    public async Task<IEnumerable<FriendRequest>> GetSentRequestsAsync(Guid senderId)
    {
        return await _dbContext.FriendRequests
            .AsNoTracking()
            .Where(fr => fr.SenderId == senderId)     // Lọc theo người gửi
            .Include(fr => fr.Receiver)               // Kèm thông tin người nhận
            .OrderByDescending(fr => fr.CreatedAt)
            .ToListAsync();
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