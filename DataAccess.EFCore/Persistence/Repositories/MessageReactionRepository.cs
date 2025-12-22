using Application.Interfaces.Repositories;
using DataAccess.EFCore.Context;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.EFCore.Persistence.Repositories;

public class MessageReactionRepository : GenericRepository<MessageReaction>, IMessageReactionRepository
{
    public MessageReactionRepository(ApplicationDbContext context) : base(context) {}
    
    public async Task<MessageReaction?> GetReactionAsync(Guid messageId, Guid userId)
    {
        return await _dbContext.Set<MessageReaction>()
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId);
    }
}