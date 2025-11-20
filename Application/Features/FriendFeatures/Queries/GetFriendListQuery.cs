using Application.DTO.Friends;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.FriendFeatures.Queries;

public record GetFriendListQuery(Guid CurrentUserId) : IRequest<IEnumerable<FriendDto>>;

public class GetFriendListQueryHandler : IRequestHandler<GetFriendListQuery, IEnumerable<FriendDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFriendListQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<FriendDto>> Handle(GetFriendListQuery request, CancellationToken cancellationToken)
    {
        // Hàm GetFriendsListAsync cần được implement trong Repository (xem Bước 3)
        var friends = await _unitOfWork.FriendRepository.GetFriendsListAsync(request.CurrentUserId);

        // Map từ Entity sang DTO
        var friendDtos = friends.Select(f =>
        {
            var friendUser = f.UserA == request.CurrentUserId ? f.FriendB : f.FriendA;
            return new FriendDto
            {
                FriendshipId = f.Id,
                FriendUserId = friendUser.Id,
                FriendUserName = friendUser.UserName,
                FriendAvatarUrl = friendUser.AvatarUrl,
                CreatedAt = f.CreatedAt
            };
        });

        return friendDtos;
    }
}