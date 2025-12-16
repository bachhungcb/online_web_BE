using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.ConversationFeatures.Commands;

public record CreateGroupConversationCommand(
    Guid CreatorId,
    string GroupName,
    string GAvatar,
    List<Guid> MemberIds
) : IRequest<Guid>;


public class CreateGroupConversationCommandHandler : IRequestHandler<CreateGroupConversationCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateGroupConversationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Guid> Handle(CreateGroupConversationCommand request, CancellationToken cancellationToken)
    {
        // 1. Validation (Input Validation - Security)
        if (string.IsNullOrWhiteSpace(request.GroupName))
            throw new Exception("Group name is required");

        if (request.MemberIds == null || !request.MemberIds.Any())
            throw new Exception("You must add at least one member");
        
        // 2. Chuẩn bị danh sách Participants
        // Phải bao gồm cả người tạo (Creator) và các thành viên được mời
        var participants = new List<Guid>();
        participants.Add(request.CreatorId); // Thêm người tạo đầu tiên
        participants.AddRange(request.MemberIds);
        
        // Loại bỏ trùng lặp (nếu Client gửi trùng ID hoặc gửi cả ID người tạo)
        participants = participants.Distinct().ToList();

        // (Tùy chọn) Kiểm tra xem các MemberIds có tồn tại trong DB không 
        // để đảm bảo tính toàn vẹn dữ liệu (Integrity).
        
        // 3. Tạo Entity Conversation
        var newGroup = new Conversation
        {
            Id = Guid.NewGuid(),
            Type = ConversationType.Group,
            Participants = participants,
            Group = new GroupCreationInfo 
            { 
                Name = request.GroupName, 
                GroupAvatar =  request.GAvatar,
                CreatedBy = request.CreatorId 
            },
            // Khởi tạo LastMessage rỗng
            LastMessage = new LastMessageInfo 
            { 
                Content = $"Group \"{request.GroupName}\" created", 
                Sender = request.CreatorId, 
                CreatedAt = DateTime.UtcNow 
            },
            SeenBy = new List<Guid> { request.CreatorId },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 4. Lưu vào DB
        _unitOfWork.ConversationRepository.Add(newGroup);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return newGroup.Id;
    }
}