using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IMessageReactionRepository : IGenericRepository<MessageReaction>
{
    Task<MessageReaction?> GetReactionAsync(Guid messageId, Guid userId);
}
