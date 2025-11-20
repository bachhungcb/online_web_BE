using Application.Interfaces.Repositories;
using DataAccess.EFCore.Context;
using Domain.Entities;

namespace DataAccess.EFCore.Persistence.Repositories;

public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
{
    public ConversationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}