using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IConversationRepository : IGenericRepository<Conversation>
{
    Task<Conversation?> GetDirectConversationAsync(Guid user1Id, Guid user2Id);
    Task<IEnumerable<Conversation>> GetConversationsByUserIdAsync(Guid userId);
}