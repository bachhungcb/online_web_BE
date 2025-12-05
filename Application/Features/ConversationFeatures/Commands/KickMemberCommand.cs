using Application.Interfaces.Repositories;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.ConversationFeatures.Commands;

public record KickMemberCommand(Guid ConversationId, Guid AdminId, Guid MemberId) : IRequest;

public class KickMemberCommandHandler : IRequestHandler<KickMemberCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubService _hubService;

    public KickMemberCommandHandler(IUnitOfWork unitOfWork, IHubService hubService)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
    }

    public async Task Handle(KickMemberCommand request, CancellationToken cancellationToken)
    {
        // 1. Lấy thông tin cuộc trò chuyện
        var conversation = await _unitOfWork.ConversationRepository.GetById(request.ConversationId);
        if (conversation == null) throw new Exception("Conversation not found");

        // 2. Kiểm tra quyền Admin (Chỉ người tạo nhóm mới được kick)
        if (conversation.Type != ConversationType.group) throw new Exception("This is not a group chat");
        
        if (conversation.Group.CreatedBy != request.AdminId)
            throw new UnauthorizedAccessException("Only the group admin can kick members.");

        // 3. Kiểm tra thành viên có trong nhóm không
        if (!conversation.Participants.Contains(request.MemberId))
            throw new Exception("Member is not in this group");

        // 4. Xóa thành viên
        conversation.Participants.Remove(request.MemberId);
        
        // 5. Tạo tin nhắn hệ thống (System Message)
        // Lấy tên người bị kick để hiển thị cho đẹp
        var kickedUser = await _unitOfWork.UserRepository.GetById(request.MemberId);
        string kickedUserName = kickedUser?.UserName ?? "Someone";
        string content = $"{kickedUserName} was removed from the group.";

        conversation.LastMessage = new LastMessageInfo
        {
            Content = content,
            Sender = Guid.Empty, // System
            CreatedAt = DateTime.UtcNow
        };
        conversation.UpdatedAt = DateTime.UtcNow;

        // Lưu Message vào bảng Messages
        var sysMsg = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.AdminId, 
            Content = content,
            MessageType = MessageType.System, // Quan trọng: Đánh dấu là tin nhắn hệ thống
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _unitOfWork.MessageRepository.Add(sysMsg);
        _unitOfWork.ConversationRepository.Update(conversation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 6. Gửi SignalR cập nhật Real-time
        await _hubService.SendMessageToGroupAsync(request.ConversationId.ToString(), new
        {
            ConversationId = request.ConversationId,
            Content = content,
            Type = MessageType.System,
            KickedMemberId = request.MemberId, // Để FE biết mà xóa user khỏi list UI ngay lập tức
            Timestamp = DateTime.UtcNow
        });
    }
}