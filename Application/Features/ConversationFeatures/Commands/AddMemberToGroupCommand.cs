using System.Reflection.Metadata;
using Application.Interfaces.Repositories;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.ConversationFeatures.Commands;

public record AddMemberToGroupCommand(
    Guid ConversationId, 
    List<Guid> NewMemberIds,
    Guid RequestorId
) : IRequest;

public class AddMemberToGroupCommandHandler : IRequestHandler<AddMemberToGroupCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubService _hubService;
    
    public AddMemberToGroupCommandHandler(IUnitOfWork unitOfWork,  IHubService hubService)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
    }

    public async Task Handle(AddMemberToGroupCommand request, CancellationToken cancellationToken)
    {
        // --- 1. Input Validation ---
        if (request.NewMemberIds == null || !request.NewMemberIds.Any())
            throw new Exception("You must select at least one member.");
        
        // --- 2. Get Data & Null Check ---
        var conversation = await _unitOfWork.ConversationRepository.GetById(request.ConversationId);
        
        if (conversation == null)
        {
            throw new Exception("Conversation not found."); // Hoặc NotFoundException
        }

        // --- 3. Authorization (Broken Access Control Prevention) ---
        // Kiểm tra 1: Người yêu cầu phải là thành viên
        if (!conversation.Participants.Contains(request.RequestorId))
        {
            throw new UnauthorizedAccessException("Access Denied. You are not a member of this conversation.");
        }
        
        // Kiểm tra 2: Chỉ cho phép thêm vào Group Chat (Chặn Direct Chat)
        if (conversation.Type != ConversationType.Group)
        {
            throw new Exception("Cannot add members to a direct conversation. Please create a new group.");
        }

        // --- 4. Business Logic & Data Integrity ---
        
        // Lọc trùng lặp (Filter duplicates):
        // Chỉ lấy những ID chưa có trong danh sách hiện tại VÀ không trùng với chính nó trong request
        var validNewMembers = request.NewMemberIds
            .Distinct() // Loại bỏ ID trùng lặp trong chính request gửi lên
            .Where(newId => !conversation.Participants.Contains(newId)) // Loại bỏ ID đã có trong nhóm
            .ToList();

        if (!validNewMembers.Any())
        {
            // Nếu lọc xong mà không còn ai (do đã ở trong nhóm hết rồi)
            // Có thể return luôn để đỡ tốn công query DB tiếp
            return; 
        }

        // (Tùy chọn - Recommended) Kiểm tra các ID này có tồn tại trong bảng Users không
        // Để tránh lưu ID rác vào JSON
        
        var existingUsersCount = await _unitOfWork.UserRepository
            .CountAsync(u => validNewMembers.Contains(u.Id)); 
        if (existingUsersCount != validNewMembers.Count) 
             throw new Exception("Some user IDs are invalid.");
        

        // --- 5. Update State ---
        conversation.Participants.AddRange(validNewMembers);
        var systemMessageContent = $"Đã thêm {validNewMembers.Count} thành viên mới.";
        // Cập nhật System Message (Thông báo: "A đã thêm B vào nhóm")
        conversation.LastMessage = new LastMessageInfo
        {
            Content = $"Added {validNewMembers.Count} new member(s).",
            MessageType = MessageType.System,
            Sender = request.RequestorId,
            CreatedAt = DateTime.UtcNow
        };
        conversation.UpdatedAt = DateTime.UtcNow;

        // --- 6. Save to DB ---
        _unitOfWork.ConversationRepository.Update(conversation);
        
        var sysMsg = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.RequestorId, // Hoặc một Guid System đặc biệt
            MessageType = MessageType.System,
            Content = systemMessageContent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _unitOfWork.MessageRepository.Add(sysMsg);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // 4. Bắn SignalR (Real-time Notification)
        // Cấu trúc object này nên khớp với những gì Frontend đang mong đợi (như trong MessagesController)
        var signalRMessage = new
        {
            SenderId = request.RequestorId,
            // Với tin nhắn hệ thống, ReceiverId có thể không quan trọng hoặc để null
            ReceiverId = Guid.Empty, 
            Content = systemMessageContent,
            Timestamp = conversation.UpdatedAt,
            conversationId = request.ConversationId,
            // Cờ để FE biết đây là tin nhắn hệ thống (nếu FE hỗ trợ)
            IsSystemMessage = true 
        };

        await _hubService.SendMessageToGroupAsync(request.ConversationId.ToString(), signalRMessage);
    }
}