using Application.DTO.Messages;
using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.MessageFeatures.Queries;

public record GetMessagesByConversationIdQuery(
    Guid ConversationId,
    Guid CurrentUserId,
    int PageNumber,
    int PageSize
) : IRequest<IEnumerable<MessageDto>>; 

public class GetMessagesByConversationIdQueryHandler 
    : IRequestHandler<GetMessagesByConversationIdQuery, IEnumerable<MessageDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMessagesByConversationIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<IEnumerable<MessageDto>> Handle(
        GetMessagesByConversationIdQuery request, 
        CancellationToken cancellationToken)
    {
        // 1. Kiểm tra người dùng có thuộc cuộc trò chuyện này không
        var conversation = await _unitOfWork.ConversationRepository.GetById(request.ConversationId);
        
        if (conversation == null)
        {
            // Trả về rỗng hoặc throw exception tùy bạn
            return new List<MessageDto>(); 
        }
        
        // 2. Gọi hàm Repository chuyên biệt vừa viết ở Bước 2
        var messages = await _unitOfWork.MessageRepository.GetMessagesByConversationIdAsync(
            request.ConversationId,
            request.PageNumber,
            request.PageSize
        );
        
        if (!conversation.Participants.Contains(request.CurrentUserId))
        {
            throw new Exception("Access denied. You are not a member of this conversation.");
        }
        // Lưu ý: messages có thể rỗng (cuộc trò chuyện chưa có tin nhắn), 
        // nhưng không được null. Nếu null thì Repository đang sai.
        
        // 3. Map từ Entity sang DTO
        var messageDtos = messages.Select(m => new MessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderName = m.Sender?.UserName ?? "Unknown", // Lấy từ bảng User nhờ .Include()
            SenderAvatarUrl = m.Sender?.AvatarUrl,
            Content = m.Content,
            CreatedAt = m.CreatedAt
        });
        
        // Nếu làm Chat App, thường ta sẽ Reverse lại list để hiển thị từ cũ đến mới ở Client
        // (Vì lúc query ta OrderByDescending để lấy tin mới nhất)
        return messageDtos.Reverse(); 
    }
}