using Application.DTO.Friends;
using Application.DTO.Users;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.UserFeatures.Queries;

public record GetUserByIdQuery(Guid userId) : IRequest<UserSummaryDto>
{


    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserSummaryDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetUserByIdQueryHandler(
            IUserRepository userRepository, 
            IFriendRepository friendRepository, 
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {

            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<UserSummaryDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = request.userId;
    
            // 1. Lấy thông tin User từ UnitOfWork
            var user = await _unitOfWork.UserRepository.GetById(userId);
            if (user == null) return null; // Hoặc ném Exception NotFound

            // 2. Khởi tạo DTO (KHÔNG gán Friends tại đây để tránh lỗi type mismatch)
            var userDto = new UserSummaryDto
            {
                Id = user.Id,
                AvatarUrl = user.AvatarUrl,
                IsOnline = user.IsOnline,
                LastActive = user.LastActive,
                UserName = user.UserName,
                // Friends = user.Friendships <-- Đã xóa dòng gây lỗi này
            };
    
            // 3. Lấy danh sách Friend Entity kèm thông tin user liên quan (FriendA/FriendB)
            // UnitOfWork gọi Repository đã được implement logic Include user
            var friendsEntities = await _unitOfWork.FriendRepository.GetFriendsListAsync(userId);
    
            // 4. Logic mapping từ Entity sang FriendDto
            var friendDtoList = new List<FriendDto>();
    
            foreach (var item in friendsEntities)
            {
                // Logic: Nếu mình là A thì bạn là B, ngược lại nếu mình là B thì bạn là A
                // Kiểm tra null để tránh NullReferenceException
                var friendUser = item.UserA == userId ? item.FriendB : item.FriendA;
    
                if(friendUser != null)
                {
                    friendDtoList.Add(new FriendDto
                    {
                        // Map đúng các trường trong FriendDTO
                        Id = friendUser.Id,               // ID của User bạn bè (dùng cho frontend điều hướng)
                        FriendUserId = friendUser.Id,     // ID cụ thể của user bạn bè
                        FriendshipId = item.Id,           // ID của mối quan hệ (bảng Friend)
                        FriendUserName = friendUser.UserName,
                        FriendAvatarUrl = friendUser.AvatarUrl,
                        CreatedAt = item.CreatedAt        // Ngày kết bạn (nếu BaseEntity có trường này)
                    });
                }
            }

            // 5. Gán danh sách đã map vào kết quả trả về
            userDto.Friends = friendDtoList;

            return userDto;
        }
    }

}