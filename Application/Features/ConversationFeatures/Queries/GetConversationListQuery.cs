using Application.DTO.Conversations;
using Application.DTO.Users;
using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ConversationFeatures.Queries;

public record GetConversationListQuery(Guid CurrentUserId)
    : IRequest<IEnumerable<ConversationDto>>;

public class GetConversationListQueryHandler
    : IRequestHandler<GetConversationListQuery, IEnumerable<ConversationDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetConversationListQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ConversationDto>> Handle(
        GetConversationListQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Lấy danh sách cuộc trò chuyện từ DB
        var conversations = await _unitOfWork.ConversationRepository
            .GetConversationsByUserIdAsync(request.CurrentUserId);

        // 2. Lấy danh sách ID của "người kia" trong các cuộc chat DIRECT
        // Để query thông tin User một lần (tránh lỗi N+1 Query)
        var directChatUserIds = conversations
            .SelectMany(c => c.Participants)
            .Concat(conversations.Select(c=> c.LastMessage.Sender))
            .Distinct()
            .Where(id=> id != Guid.Empty)
            .ToList();

        // 3. Lấy thông tin Users từ DB
        var usersDict = (await _unitOfWork.UserRepository.GetAllAsQueryable()
                .Where(u => directChatUserIds.Contains(u.Id))
                .ToListAsync(cancellationToken)) // ToListAsync trả về List<User>
            .ToDictionary(u => u.Id); // Chuyển sang Dictionary để tra cứu cho nhanh

        // 4. Map sang DTO
        var dtos = new List<ConversationDto>();

        foreach (var convo in conversations)
        {
            string name = "Unknown";
            string avatar = "";
            // Logic lấy danh sách participants cho DTO
            var participantDtos = convo.Participants
                .Where(uid => usersDict.ContainsKey(uid)) // Check an toàn
                .Select(uid =>
                {
                    var u = usersDict[uid];
                    return new UserSummaryDto
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        AvatarUrl = u.AvatarUrl
                    };
                }).ToList();
            
            //---- Last Sender Info ----
            string lastSenderName = "";
            Guid lastSenderId = Guid.Empty;
            string lastSenderAvatarUrl = "";
            
            if (convo.LastMessage != null && convo.LastMessage.Sender != Guid.Empty)
            {
                lastSenderId = convo.LastMessage.Sender;
            
                // Nếu chính mình gửi thì hiển thị "Bạn:" hoặc "You:"
                if (lastSenderId == request.CurrentUserId)
                {
                    lastSenderName = "You";
                }
                else if (usersDict.TryGetValue(lastSenderId, out var senderUser))
                {
                    // Nếu người khác gửi, lấy tên từ Dict
                    // Với group chat, hiển thị tên người gửi. Với direct, cũng hiển thị tên.
                    lastSenderName = senderUser.UserName; // Hoặc senderUser.FullName
                    lastSenderAvatarUrl = senderUser.AvatarUrl;
                }
                else 
                {
                    lastSenderName = "Unknown"; // Trường hợp user bị xóa
                }
            }
            
            
            if (convo.Type == ConversationType.Group)
            {
                name = convo.Group?.Name ?? "Unnamed Group";
                // avatar = convo.Group.AvatarUrl ... (nếu có)
            }
            else // Direct
            {
                // Tìm ID người kia
                var partnerId = convo.Participants.FirstOrDefault(p => p != request.CurrentUserId);

                // Tra cứu thông tin trong Dictionary đã lấy ở bước 3
                if (usersDict.TryGetValue(partnerId, out var partner))
                {
                    name = partner.FullName; // Hoặc UserName
                    avatar = partner.AvatarUrl;
                }
            }

            dtos.Add(new ConversationDto
            {
                Id = convo.Id,
                Name = name,
                AvatarUrl = avatar,
                LastMessageContent = convo.LastMessage?.Content ?? "Start a conversation",
                LastMessageTime = convo.LastMessage?.CreatedAt ?? convo.CreatedAt,
                LastMessageSenderName = lastSenderName,
                LastMessageSenderId = lastSenderId,
                LastMessageSenderAvatarUrl = lastSenderAvatarUrl,
                // Kiểm tra xem mình đã xem chưa
                IsRead = convo.SeenBy.Contains(request.CurrentUserId),
                Participants = participantDtos,
            });
        }

        return dtos;
    }
}