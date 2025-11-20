using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IMessageRepository : IGenericRepository<Message>
{
    Task<IEnumerable<Message>> GetMessagesByConversationIdAsync(
        Guid conversationId, 
        int pageNumber, 
        int pageSize
    );
}