using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.ConversationFeatures.Commands;

public record CreateConversationCommand(
    Guid SenderId,
    Guid ReceiverId
    ) : IRequest<Guid>;

public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateConversationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
    {
        // 1. Không thể chat với chính mình
        if (request.SenderId == request.ReceiverId)
            throw new Exception("Cannot create conversation with yourself");

        // 2. Kiểm tra người nhận có tồn tại không
        var receiver = await _unitOfWork.UserRepository.GetById(request.ReceiverId);
        if (receiver == null)
            throw new Exception("User not found");

        // 3. Kiểm tra xem đã có cuộc trò chuyện cũ chưa
        var existingConversation = await _unitOfWork.ConversationRepository
            .GetDirectConversationAsync(request.SenderId, request.ReceiverId);

        if (existingConversation != null)
        {
            // Nếu đã có, trả về ID cũ luôn (không tạo mới)
            return existingConversation.Id;
        }

        // 4. Nếu chưa có, tạo mới
        var newConversation = new Conversation
        {
            Id = Guid.NewGuid(),
            Participants = new List<Guid> { request.SenderId, request.ReceiverId },
            Type = ConversationType.direct,
            // Khởi tạo các giá trị required
            Group = new GroupCreationInfo { Name = "", CreatedBy = Guid.Empty }, // Direct chat không cần Group info
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