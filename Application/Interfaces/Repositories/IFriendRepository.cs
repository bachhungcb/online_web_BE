using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IFriendRepository : IGenericRepository<Friend>
{
    Task<IEnumerable<Friend>> GetFriendsListAsync(Guid userId);
    Task<Friend?> GetFriendshipAsync(Guid userId1, Guid userId2);
    Task<bool> IsFriendAsync(Guid userId1, Guid userId2);
}