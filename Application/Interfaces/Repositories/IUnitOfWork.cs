namespace Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
   IUserRepository UserRepository { get; }
   IFriendRepository FriendRepository { get; }
   IFriendRequestRepository  FriendRequestRepository { get; }
   Task<int> SaveChangesAsync(CancellationToken cancellationToken);
   Task<int> Complete();
}