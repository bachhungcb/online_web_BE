using Application.DTO.Conversations;
using Application.DTO.Users;
using Application.Features.UserFeatures.Queries;
using Application.Interfaces.Repositories;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.ConversationFeatures.Commands;

public record CreateRandomConversationCommand(
    Guid SenderId
) : IRequest<ConversationSummaryDto>;

public class CreateRandomConversationCommandHandler : IRequestHandler<CreateRandomConversationCommand, ConversationSummaryDto>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubService _hubService;

    public CreateRandomConversationCommandHandler(IUnitOfWork unitOfWork, IMediator mediator, IHubService hubService)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _hubService = hubService;
    }


    public async Task<ConversationSummaryDto> Handle(CreateRandomConversationCommand request, CancellationToken cancellationToken)
    {
        var userIds = await _mediator.Send(new GetListUserIdQuery(request.SenderId), cancellationToken);


        if (userIds.Count == 0)
        {
            throw new Exception("No other users found to chat with.");
        }

        var random = new Random();
        int index = random.Next(userIds.Count);
        var receiverId = userIds[index];

        // 1. Không thể chat với chính mình
        if (request.SenderId == receiverId)
            throw new Exception("Cannot create conversation with yourself");

        // 2. Kiểm tra người nhận có tồn tại không
        var receiver = await _unitOfWork.UserRepository.GetById(receiverId);
        if (receiver == null)
            throw new Exception("User not found");
        
        var sender = await _unitOfWork.UserRepository.GetById(request.SenderId);
        if (sender == null)
            throw new Exception("sender not found");
        
        
        // 3. Kiểm tra xem đã có cuộc trò chuyện cũ chưa
        var existingConversation = await _unitOfWork.ConversationRepository
            .GetDirectConversationAsync(request.SenderId, receiverId);

        if (existingConversation != null)
        {
            // Nếu đã có, trả về ID cũ luôn (không tạo mới)
            var dto = new ConversationSummaryDto
            {
                ConversationId =  existingConversation.Id,
                Name = receiver.UserName,
                AvatarUrl =  receiver.AvatarUrl,
            };
            
            var realTimeMsg = new
            {
                ConversationId = existingConversation.Id,
                Name = receiver.UserName,
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
            Participants = new List<Guid> { request.SenderId, receiverId },
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

        var summaryDto = new ConversationSummaryDto
        {
            ConversationId =  newConversation.Id,
            Name = receiver.UserName,
            AvatarUrl =  receiver.AvatarUrl,
        };
        // 5. Sending signalr
        var signalRMessage = new
        {
            ConversationId = newConversation.Id,
            Name = receiver.UserName,
            AvatarUrl =  receiver.AvatarUrl,
        };
        await _hubService.SendMessageToGroupAsync(newConversation.Id.ToString(), signalRMessage);

        
        return summaryDto;
    }
}