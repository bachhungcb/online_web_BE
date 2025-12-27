using Application.Interfaces.Repositories;
using DataAccess.EFCore.Context;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.EFCore.Persistence.Repositories;

public class FriendRepository : GenericRepository<Friend>, IFriendRepository
{
    public FriendRepository(ApplicationDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<Friend>> GetFriendsListAsync(Guid userId)
    {
        return await _dbContext.Friends
            .AsNoTracking()
            .Where(f => f.UserA == userId || f.UserB == userId)
            .Include(f => f.FriendA) // <--- QUAN TRỌNG: Load thông tin User A
            .Include(f => f.FriendB) // <--- QUAN TRỌNG: Load thông tin User B
            .ToListAsync();
    }

    public Task<Friend?> GetFriendshipAsync(Guid userId1, Guid userId2)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> IsFriendAsync(Guid userId1, Guid userId2)
    {
        return await _dbContext.Friends
            .AsNoTracking()
            .AnyAsync(f => 
            (f.UserA == userId1 && f.UserB == userId2) ||
            (f.UserA == userId2 && f.UserB == userId1)
            );
    }
    
    public async Task<IEnumerable<Friend>> GetListFriendByUserIdAsync(Guid userId)
    {
        // Giả định logic: User có thể nằm ở cột SenderId hoặc ReceiverId
        // Và cần Include thông tin của người bạn đó (User còn lại)
    
        return await _dbContext.Friends
            .Include(f => f.UserA)
            .Include(f => f.UserB)
            .Where(x => (Equals(x.UserA, userId) || Equals(x.UserB, userId))) 
            // Lưu ý: Thông thường bạn bè phải có trạng thái Accepted. 
            // Nếu Entity Friend của bạn không có cột Status (mà tách ra bảng Request), thì bỏ qua điều kiện Status.
            // Dựa trên file Friend.cs bạn gửi, nó có vẻ là bảng kết quả sau khi accept.
            .ToListAsync();
    }
}