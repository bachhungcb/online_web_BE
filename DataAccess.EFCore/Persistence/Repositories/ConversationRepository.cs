using Application.Interfaces.Repositories;
using DataAccess.EFCore.Context;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.EFCore.Persistence.Repositories;

public class ConversationRepository : GenericRepository<Conversation>, IConversationRepository
{
    public ConversationRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
    
    public async Task<Conversation?> GetDirectConversationAsync(Guid user1Id, Guid user2Id)
    {
        // 1. Lấy tất cả cuộc trò chuyện loại DIRECT
        // Lưu ý: Do Participants lưu dạng String/JSON, ta cần cẩn thận khi query.
        // Cách an toàn nhất mà không cần sửa DB config phức tạp:
        
        var conversations = await _dbContext.Conversation
            .Where(c => c.Type == ConversationType.direct)
            .ToListAsync(); // Tải về RAM (nếu dữ liệu lớn cần tối ưu sau)

        // 2. Lọc thủ công trong RAM
        return conversations.FirstOrDefault(c => 
            c.Participants.Contains(user1Id) && 
            c.Participants.Contains(user2Id));
    }
    
    public async Task<IEnumerable<Conversation>> GetConversationsByUserIdAsync(Guid userId)
    {
        // 1. Lấy tất cả conversation (Do hạn chế của lưu trữ JSON/String trong SQL,
        // ta tạm thời lấy về rồi lọc. Nếu dữ liệu lớn, cần giải pháp bảng phụ ConversationParticipants)
        var conversations = await _dbContext.Conversation
            .AsNoTracking()
            .OrderByDescending(c => c.UpdatedAt) // Mới nhất lên đầu
            .ToListAsync();

        // 2. Lọc những cuộc trò chuyện có chứa userId
        return conversations.Where(c => c.Participants.Contains(userId));
    }
}