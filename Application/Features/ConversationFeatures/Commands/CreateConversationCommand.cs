using Application.DTO.Conversations;
using Application.DTO.Users;
using Application.Interfaces.Repositories;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.ConversationFeatures.Commands;

public record CreateConversationCommand(
    Guid SenderId,
    Guid ReceiverId
) : IRequest<ConversationSummaryDto>;

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubService _hubService;

    public CreateConversationCommandHandler(IUnitOfWork unitOfWork, IHubService hubService)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
    }

    public async Task<ConversationSummaryDto> Handle(CreateConversationCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Không thể chat với chính mình
        if (request.SenderId == request.ReceiverId)
            throw new Exception("Cannot create conversation with yourself");

        // 2. Kiểm tra người nhận có tồn tại không
        var receiver = await _unitOfWork.UserRepository.GetById(request.ReceiverId);
        if (receiver == null)
            throw new Exception("Receiver not found");

        var sender = await _unitOfWork.UserRepository.GetById(request.SenderId);
        if (sender == null)
            throw new Exception("sender not found");

        var receiverDto = new UserSummaryDto
        {
            Id = receiver.Id,
            AvatarUrl = receiver.AvatarUrl,
            IsOnline = receiver.IsOnline,
            LastActive = receiver.LastActive,
            UserName = receiver.UserName,
        };


        // 3. Kiểm tra xem đã có cuộc trò chuyện cũ chưa
        var existingConversation = await _unitOfWork.ConversationRepository
            .GetDirectConversationAsync(request.SenderId, request.ReceiverId);

        if (existingConversation != null)
        {
            var dto = new ConversationSummaryDto
            {
                ConversationId = existingConversation.Id,

            };

            var realTimeMsg = new
            {
                ConversationId = existingConversation.Id,
                Name = receiverDto.UserName,
                AvatarUrl =  receiver.AvatarUrl,
            };

            await _hubService.SendMessageToGroupAsync(existingConversation.Id.ToString(), realTimeMsg);
            // Nếu đã có, trả về ID cũ luôn (không tạo mới)
            return dto;
        }

        // 4. Nếu chưa có, tạo mới
        var newConversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Participants = new List<Guid> { request.SenderId, request.ReceiverId },
            Type = ConversationType.Direct,
            // Khởi tạo các giá trị required
            Group = new GroupCreationInfo
                { Name = "", CreatedBy = Guid.Empty, GroupAvatar = "" }, // Direct chat không cần Group info
            LastMessage = new LastMessageInfo // Khởi tạo rỗng
            {
                Content = "",
                Sender = Guid.Empty,
                CreatedAt = DateTime.MinValue
            },
            SeenBy = new List<Guid> { request.SenderId }, // Người tạo coi như đã xem
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _unitOfWork.ConversationRepository.Add(newConversation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Sending signalr
        var signalRMessage = new
        {
            ConversationId = newConversation.Id,
            Name = receiverDto.UserName,
            AvatarUrl =  receiver.AvatarUrl,
        };
        await _hubService.SendMessageToGroupAsync(newConversation.Id.ToString(), signalRMessage);

        var summaryDto = new ConversationSummaryDto
        {
            ConversationId = newConversation.Id,
            Name = receiverDto.UserName,
            AvatarUrl =  receiver.AvatarUrl,
        };

        return summaryDto;
    }
}