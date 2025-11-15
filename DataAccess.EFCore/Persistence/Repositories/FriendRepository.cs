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

    public Task<IEnumerable<Friend>> GetFriendsListAsync(Guid userId)
    {
        throw new NotImplementedException();
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