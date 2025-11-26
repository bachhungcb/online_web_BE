using Application.DTO.Conversations;
using Application.DTO.Users;
using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ConversationFeatures.Queries;

public record GetConversationByIdQuery(Guid ConversationId, Guid CurrentUserId)
    : IRequest<ConversationDto>
{
    public class GetConversationByIdQueryHandler : IRequestHandler<GetConversationByIdQuery, ConversationDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetConversationByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ConversationDto> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
        {
            // 1. Get conversation from DB
            var conversation = await _unitOfWork.ConversationRepository.GetById(request.ConversationId);

            // 2. Validate data
            if (conversation == null)
            {
                throw new Exception("Can NOT find conversation");
            }

            // Check Authorization
            if (!conversation.Participants.Contains(request.CurrentUserId))
            {
                throw new UnauthorizedAccessException("Not authorized");
            }

            // 3. Get Participant data
            var participantIds = conversation.Participants;

            var usersDict = (await _unitOfWork.UserRepository.GetAllAsQueryable()
                    .Where(u => participantIds.Contains(u.Id))
                    .ToListAsync(cancellationToken))
                .ToDictionary(u => u.Id);
            // 4. Xử lý Logic hiển thị (Tên & Avatar)
            string name = "Unknown";
            string avatar = "";

            if (conversation.Type == ConversationType.group)
            {
                name = conversation.Group?.Name ?? "Unnamed Group";
                // avatar = conversation.Group?.AvatarUrl;
            }
            else // Direct Chat
            {
                // Tìm ID người kia (Partner)
                var partnerId = conversation.Participants.FirstOrDefault(p => p != request.CurrentUserId);

                // Lấy thông tin từ Dictionary
                if (usersDict.TryGetValue(partnerId, out var partner))
                {
                    name = partner.FullName;
                    avatar = partner.AvatarUrl;
                }
            }

            // 5. Map sang DTO và Return
            // Lưu ý: Không dùng List<Dto> như code cũ của bạn, vì ta chỉ trả về 1 object
            var dto = new ConversationDto
            {
                Id = conversation.Id,
                Name = name,
                AvatarUrl = avatar,
                LastMessageContent = conversation.LastMessage?.Content,
                LastMessageTime = conversation.LastMessage?.CreatedAt ?? conversation.CreatedAt,
                IsRead = conversation.SeenBy.Contains(request.CurrentUserId),

                // Map danh sách participants (nếu DTO của bạn có field này như đã bàn ở câu trước)
                Participants = conversation.Participants
                    .Where(uid => usersDict.ContainsKey(uid))
                    .Select(uid => new UserSummaryDto
                    {
                        Id = uid,
                        UserName = usersDict[uid].UserName,
                        AvatarUrl = usersDict[uid].AvatarUrl
                    }).ToList()
            };

            return dto;
        }
    }
}