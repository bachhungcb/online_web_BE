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
            .AsNoTracking() // Nên thêm AsNoTracking nếu chỉ để đọc (tối ưu hiệu năng)
            .Include(f => f.FriendA) // <--- SỬA LẠI: Dùng FriendA (User) thay vì UserA (Guid)
            .Include(f => f.FriendB) // <--- SỬA LẠI: Dùng FriendB (User) thay vì UserB (Guid)
            .Where(x => x.UserA == userId || x.UserB == userId)
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
    
}