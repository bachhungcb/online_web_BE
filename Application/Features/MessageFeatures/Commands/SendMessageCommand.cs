using Application.DTO.Messages;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace Application.Features.MessageFeatures.Commands;

public record SendMessageCommand(
    Guid SenderId,
    Guid ConversationId,
    string? Content,
    List<string>? MediaUrls,
    MessageType Type) : IRequest<MessageSentResultDto>;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, MessageSentResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    // Inject HubContext để bắn tin nhắn từ bên ngoài Hub
    // Lưu ý: Để dùng được Hub trong Project Application, bạn nên tạo Interface IHubService trong Application 
    // và implement nó ở Api, nhưng để đơn giản demo, tôi giả định bạn tham chiếu thư viện SignalR.
    // Cách chuẩn Clean Arch: Tạo interface INotificationService ở Application.

    // Tạm thời demo logic xử lý dữ liệu:
    public SendMessageCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MessageSentResultDto> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra Conversation có tồn tại và User có trong đó không
        var conversation = await _unitOfWork.ConversationRepository.GetById(request.ConversationId);
        if (conversation == null) throw new Exception("Conversation not found");
        var mediaUrls = request.MediaUrls ?? new List<string>();
        
        Guid receiverId = Guid.Empty;
        string receiverName = "";
        string receiverAvatarUrl = "";

        // Get Sender info
        var sender = await _unitOfWork.UserRepository.GetById(request.SenderId);
        // Check participant
        if (!conversation.Participants.Contains(request.SenderId))
        {
            throw new Exception("You are not a member of this conversation");
        }

        if (string.IsNullOrWhiteSpace(request.Content) && !mediaUrls.Any())
        {
            throw new Exception("Message must contain text or media.");
        }

        if (conversation.Type == ConversationType.Direct)
        {
            // Tìm ID của người kia (Người nhận) trong danh sách Participants
            // Participants chứa [SenderId, ReceiverId], ta lọc lấy người không phải SenderId
            receiverId = conversation.Participants.FirstOrDefault(p => p != request.SenderId);

            if (receiverId != Guid.Empty)
            {
                // Check bạn bè
                // var isFriend = await _unitOfWork.FriendRepository.IsFriendAsync(request.SenderId, receiverId);
                // if (!isFriend) throw new Exception("Message sending failed. You are not friends with this user.");
            
                // Lấy info người nhận
                var receiverUser = await _unitOfWork.UserRepository.GetById(receiverId);
                if (receiverUser != null)
                {
                    receiverName = receiverUser.UserName;
                    receiverAvatarUrl = receiverUser.AvatarUrl;
                }
            }
        }else 
        {
            // === LOGIC CHO GROUP CHAT ===
            receiverId = conversation.Id;
            receiverName = conversation.Group?.Name ?? "Unknown Group";
            //receiver.AvatarUrl = conversation.Group?.AvatarUrl; // Nếu nhóm có avatar
        }

        // 2. Tạo Message Entity
        var message = new Message
        {
            SenderId = request.SenderId,
            ConversationId = request.ConversationId,
            Content = request.Content,
            MediaUrls = request.MediaUrls,
            MessageType = request.Type,
            CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
        };

        // 3. Cập nhật LastMessage cho Conversation (Để hiển thị ở danh sách chat)
        // Entity Conversation của bạn có Owned Type LastMessageInfo

        string previewContent = "";
        
        if (!string.IsNullOrEmpty(request.Content))
        {
            previewContent = request.Content;
        }
        else if (mediaUrls.Count > 0)
        {
            // Ví dụ: "[3 Images]" hoặc "[Video]"
            string typeName = request.Type == MessageType.Image ? "Photos" : "Files";
            previewContent = $"[{mediaUrls.Count} {typeName}]";
        }
        else
        {
            previewContent = "Sent a message";
        }
        
        if (conversation.Type == ConversationType.Group)
        {
            previewContent = $"{sender.UserName}: {previewContent}"; // Tùy chọn
        }

        conversation.LastMessage = new LastMessageInfo
        {
            Content = previewContent,
            Sender = request.SenderId,
            CreatedAt = message.CreatedAt
        };
        conversation.UpdatedAt = DateTime.UtcNow; // Đẩy cuộc trò chuyện lên đầu

        conversation.SeenBy = new List<Guid> { request.SenderId };

        // 4. Lưu vào DB (Transaction)
        _unitOfWork.MessageRepository.Add(message);
        _unitOfWork.ConversationRepository.Update(conversation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Real-time Notification (Phần này nên làm ở Controller hoặc Service riêng để tránh dependency circular)
        // Tuy nhiên, logic là: Sau khi save thành công -> Bắn SignalR
        return new MessageSentResultDto
        {
            MessageId = message.Id, // Nên trả về MessageId
            SenderId = request.SenderId,
            SenderAvatarUrl = sender?.AvatarUrl,
        
            // Các trường này đã được xử lý ở bước if/else trên
            ReceiverId = receiverId, 
            ReceiverUserName = receiverName, 
            ReceiverAvatarUrl = receiverAvatarUrl,
        
            Content = request.Content,
            MediaUrls = mediaUrls, // Trả về list media để FE hiển thị ngay
            CreatedAt = DateTime.SpecifyKind(message.CreatedAt, DateTimeKind.Utc),
        };
    }
}