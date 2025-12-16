using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.ConversationFeatures.Commands;

public record MarkConversationAsReadCommand(Guid ConversationId, Guid UserId) : IRequest<bool>;

public class MarkConversationAsReadCommandHandler : IRequestHandler<MarkConversationAsReadCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public MarkConversationAsReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(MarkConversationAsReadCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _unitOfWork.ConversationRepository.GetById(request.ConversationId);
        if (conversation == null) throw new Exception("Conversation not found");

        // Nếu user chưa có trong danh sách SeenBy thì thêm vào
        if (!conversation.SeenBy.Contains(request.UserId))
        {
            conversation.SeenBy.Add(request.UserId);
            _unitOfWork.ConversationRepository.Update(conversation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
        return false;
    }
}