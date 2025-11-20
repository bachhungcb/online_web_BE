using Application.Interfaces.Repositories;
using DataAccess.EFCore.Context;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.EFCore.Persistence.Repositories;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
    
    public async Task<IEnumerable<Message>> GetMessagesByConversationIdAsync(
        Guid conversationId, 
        int pageNumber, 
        int pageSize)
    {
        return await _dbContext.Messages
            .AsNoTracking() // Quan trọng: Tối ưu hiệu năng đọc
            .Where(m => m.ConversationId == conversationId)
            .Include(m => m.Sender) // Include để lấy thông tin SenderName, Avatar
            .OrderByDescending(m => m.CreatedAt) // Lấy tin mới nhất trước
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}