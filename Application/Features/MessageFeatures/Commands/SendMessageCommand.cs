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
    string Content) : IRequest<MessageSentResultDto>;

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
        Guid receiverId = Guid.Empty;
        User receiver = null;
        // Check participant
        if (!conversation.Participants.Contains(request.SenderId))
        {
            throw new Exception("You are not a member of this conversation");
        }

        if (conversation.Type == ConversationType.direct)
        {
            // Tìm ID của người kia (Người nhận) trong danh sách Participants
            // Participants chứa [SenderId, ReceiverId], ta lọc lấy người không phải SenderId
            receiverId = conversation.Participants.FirstOrDefault(p => p != request.SenderId);

            // Nếu tìm thấy người nhận (trường hợp bình thường)
            if (receiverId != Guid.Empty)
            {
                // Gọi Repository để kiểm tra quan hệ bạn bè
                var isFriend = await _unitOfWork.FriendRepository.IsFriendAsync(request.SenderId, receiverId);
                receiver = await _unitOfWork.UserRepository.GetById(receiverId);
                if (!isFriend)
                {
                    // Nếu không phải bạn bè -> Chặn luôn
                    throw new Exception("Message sending failed. You are not friends with this user.");
                }
            }
        }

        // 2. Tạo Message Entity
        var message = new Message
        {
            SenderId = request.SenderId,
            ConversationId = request.ConversationId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 3. Cập nhật LastMessage cho Conversation (Để hiển thị ở danh sách chat)
        // Entity Conversation của bạn có Owned Type LastMessageInfo
        conversation.LastMessage = new LastMessageInfo
        {
            Content = request.Content,
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
            SenderId = request.SenderId,
            ReceiverId = receiverId,
            ReceiverUserName = receiver.UserName,
            ReceiverAvatarUrl = receiver.AvatarUrl,
            Content = request.Content,
            CreatedAt = message.CreatedAt,
        };
    }
}