using Application.Interfaces.Repositories;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.ConversationFeatures.Commands;

public record ChangeGroupNameCommand(Guid ConversationId, Guid RequestorId, string NewName) : IRequest;

public class ChangeGroupNameCommandHandler : IRequestHandler<ChangeGroupNameCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubService _hubService;

    public ChangeGroupNameCommandHandler(IUnitOfWork unitOfWork, IHubService hubService)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
    }

    public async Task Handle(ChangeGroupNameCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _unitOfWork.ConversationRepository.GetById(request.ConversationId);
        if (conversation == null) throw new Exception("Conversation not found");

        if (conversation.Type != ConversationType.group) throw new Exception("Only groups can be renamed");

        // Validate: Người đổi tên phải là thành viên nhóm
        if (!conversation.Participants.Contains(request.RequestorId))
            throw new UnauthorizedAccessException("You are not a member of this group");

        // Logic đổi tên
        string oldName = conversation.Group.Name;
        conversation.Group.Name = request.NewName;

        // Tạo tin nhắn hệ thống
        var requestor = await _unitOfWork.UserRepository.GetById(request.RequestorId);
        string requestorName = requestor?.UserName ?? "Someone";
        string content = $"{requestorName} changed group name to \"{request.NewName}\".";

        conversation.LastMessage = new LastMessageInfo
        {
            Content = content,
            Sender = Guid.Empty,
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
            NewGroupName = request.NewName, // Để FE cập nhật Header ngay lập tức
            Timestamp = DateTime.UtcNow
        });
    }
}