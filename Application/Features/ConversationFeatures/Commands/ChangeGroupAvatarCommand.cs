using Application.Interfaces.Repositories;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.ConversationFeatures.Commands;

public record ChangeGroupAvatarCommand(Guid ConversationId, Guid RequestorId, string NewAvatar) : IRequest;

public class ChangeGroupAvatarCommandHandler : IRequestHandler<ChangeGroupAvatarCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubService _hubService;

    public ChangeGroupAvatarCommandHandler(IUnitOfWork unitOfWork, IHubService hubService)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
    }

    public async Task Handle(ChangeGroupAvatarCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _unitOfWork.ConversationRepository.GetById(request.ConversationId);
        if (conversation == null) throw new Exception("Conversation not found");

        
        // Validate: Người đổi tên phải là thành viên nhóm
        if (!conversation.Participants.Contains(request.RequestorId))
            throw new UnauthorizedAccessException("You are not a member of this group");

        // Logic đổi tên
        conversation.Group.Name = request.NewAvatar;

        // Tạo tin nhắn hệ thống
        var requestor = await _unitOfWork.UserRepository.GetById(request.RequestorId);
        string requestorName = requestor?.UserName ?? "Someone";
        string content = $"{requestorName} changed group avatar to \"{request.NewAvatar}\".";

        conversation.LastMessage = new LastMessageInfo
        {
            Content = content,
            Sender = Guid.Empty,
            MessageType = MessageType.System,
            CreatedAt = DateTime.UtcNow
        };
        conversation.UpdatedAt = DateTime.UtcNow;

        var sysMsg = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = request.ConversationId,
            SenderId = request.RequestorId,
            Content = content,
            MessageType = MessageType.System,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _unitOfWork.MessageRepository.Add(sysMsg);
        _unitOfWork.ConversationRepository.Update(conversation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Gửi SignalR
        await _hubService.SendMessageToGroupAsync(request.ConversationId.ToString(), new
        {
            ConversationId = request.ConversationId,
            Content = content,
            Type = MessageType.System,
            NewGroupAvatar = request.NewAvatar, // Để FE cập nhật Header ngay lập tức
            Timestamp = DateTime.UtcNow
        });
    }
}