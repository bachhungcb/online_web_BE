using Application.Interfaces.Repositories;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.ConversationFeatures.Commands;

public record LeaveGroupCommand(
    Guid ConversationId,
    Guid UserId) : IRequest;

public class LeaveGroupCommandHandler : IRequestHandler<LeaveGroupCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubService _hubService;

    public LeaveGroupCommandHandler(IUnitOfWork unitOfWork, IHubService hubService)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
    }

    public async Task Handle(LeaveGroupCommand request, CancellationToken cancellationToken)
    {
       // --- 1. Validation & Get Data ---
        if (request.UserId == Guid.Empty) throw new Exception("UserId required.");

        var conversation = await _unitOfWork.ConversationRepository.GetById(request.ConversationId);
        if (conversation == null) throw new Exception("Conversation not found.");

        // --- 2. Authorization Checks ---
        if (!conversation.Participants.Contains(request.UserId))
            throw new UnauthorizedAccessException("You are not a member of this group.");
        
        if (conversation.Type != ConversationType.Group)
            throw new Exception("Cannot leave a direct conversation.");

        // Lấy thông tin User để ghi log tên người rời đi
        var leavingUser = await _unitOfWork.UserRepository.GetById(request.UserId);
        string leaverName = leavingUser?.UserName ?? "Someone";

        // --- 3. Process Leaving Logic ---
        
        // Xóa User khỏi danh sách
        conversation.Participants.Remove(request.UserId);

        // CASE 1: Nhóm không còn ai -> Xóa nhóm (Clean up Zombie Data)
        if (conversation.Participants.Count == 0)
        {
            _unitOfWork.ConversationRepository.Remove(conversation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return; // Kết thúc luôn, không cần bắn noti
        }

        // CASE 2: Nhóm vẫn còn người
        // Kiểm tra xem người rời đi có phải Admin (CreatedBy) không?
        if (conversation.Group.CreatedBy == request.UserId)
        {
            // Chuyển quyền Admin cho người đầu tiên còn lại trong danh sách
            // (Business Rule: Promote next member to Admin)
            var newAdminId = conversation.Participants.First();
            conversation.Group.CreatedBy = newAdminId;
        }

        // --- 4. System Message & Update State ---
        var systemMsgContent = $"{leaverName} left the group.";

        conversation.LastMessage = new LastMessageInfo
        {
            Content = systemMsgContent,
            Sender = Guid.Empty, // System Sender
            MessageType =  MessageType.System,
            CreatedAt = DateTime.UtcNow
        };
        conversation.UpdatedAt = DateTime.UtcNow;

        // Lưu log message vào bảng Message (để hiển thị lịch sử)
        var sysMsgEntity = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.UserId, // Hoặc để null/Guid.Empty tùy quy ước
            Content = systemMsgContent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _unitOfWork.MessageRepository.Add(sysMsgEntity);
        
        // Update Conversation
        _unitOfWork.ConversationRepository.Update(conversation);
        
        // --- 5. Save & Notify ---
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Bắn SignalR cho những người Ở LẠI
        await _hubService.SendMessageToGroupAsync(request.ConversationId.ToString(), new
        {
            Content = systemMsgContent,
            conversationId = request.ConversationId,
            Timestamp = DateTime.UtcNow,
            IsSystemMessage = true,
            LeftUserId = request.UserId, // Để FE biết mà xóa avatar user này khỏi UI
            NewAdminId = conversation.Group.CreatedBy // Để FE cập nhật icon vương miện/key (nếu có đổi)
        });
    }
}