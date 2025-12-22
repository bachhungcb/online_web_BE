using Application.Interfaces.Repositories;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.MessageFeatures.Commands;

public record ReactToMessageCommand(Guid MessageId, Guid UserId, ReactionType ReactionType) : IRequest<string>;

public class ReactToMessageCommandHandler : IRequestHandler<ReactToMessageCommand, string>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubService _hubService;

    public ReactToMessageCommandHandler(IUnitOfWork unitOfWork, IHubService hubService)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
    }

    public async Task<string> Handle(ReactToMessageCommand request, CancellationToken cancellationToken)
    {
        // 1. Validation
        var message = await _unitOfWork.MessageRepository.GetById(request.MessageId);
        if (message == null) throw new Exception("Message not found");

        var conversation = await _unitOfWork.ConversationRepository.GetById(message.ConversationId);
        if (!conversation.Participants.Contains(request.UserId))
        {
            throw new UnauthorizedAccessException("Access Denied: You are not in this conversation.");
        }

        // 2. Check if user is already reacted the message
        var existingReaction = await _unitOfWork.MessageReactionRepository
            .GetReactionAsync(request.MessageId, request.UserId);

        string action = ""; // "Added", "Removed", "Updated"

        if (existingReaction != null)
        {
            if (existingReaction.ReactionType == request.ReactionType)
            {
                // Case 1: Đã thả icon này rồi -> Gỡ bỏ (Toggle Off)
                _unitOfWork.MessageReactionRepository.Remove(existingReaction);
                action = "Removed";
            }
            else
            {
                // Case 2: Đã thả icon khác -> Cập nhật icon mới
                existingReaction.ReactionType = request.ReactionType;
                _unitOfWork.MessageReactionRepository.Update(existingReaction);
                action = "Updated";
            }
        }
        else
        {
            // Case 3: Chưa thả -> Thêm mới
            var newReaction = new MessageReaction
            {
                Id = Guid.NewGuid(),
                MessageId = request.MessageId,
                UserId = request.UserId,
                ReactionType = request.ReactionType,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.MessageReactionRepository.Add(newReaction);
            action = "Added";
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 3. Real-time Notification (SignalR)
        // Gửi payload gọn nhẹ để Client update UI ngay lập tức
        await _hubService.SendMessageToGroupAsync(message.ConversationId.ToString(), new
        {
            Type = "ReactionUpdate", // Client dựa vào đây để switch case
            MessageId = request.MessageId,
            UserId = request.UserId,
            ReactionType = request.ReactionType,
            Action = action // Để Client biết là thêm hay xóa
        });

        return action;
    }
}