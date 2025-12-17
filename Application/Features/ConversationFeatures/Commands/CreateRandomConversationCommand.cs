using Application.Features.UserFeatures.Queries;
using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.ConversationFeatures.Commands;

public record CreateRandomConversationCommand(
    Guid SenderId
) : IRequest<Guid>;

public class CreateRandomConversationCommandHandler : IRequestHandler<CreateRandomConversationCommand, Guid>
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRandomConversationCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }


    public async Task<Guid> Handle(CreateRandomConversationCommand request, CancellationToken cancellationToken)
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

        // 3. Kiểm tra xem đã có cuộc trò chuyện cũ chưa
        var existingConversation = await _unitOfWork.ConversationRepository
            .GetDirectConversationAsync(request.SenderId, receiverId);

        if (existingConversation != null)
        {
            // Nếu đã có, trả về ID cũ luôn (không tạo mới)
            return existingConversation.Id;
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

        return newConversation.Id;
    }
}