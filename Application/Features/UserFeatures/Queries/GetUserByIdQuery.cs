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
            var Id = request.userId;
            
            var user = await _unitOfWork.UserRepository.GetById(Id);
            if (user == null) return null;

            var userDto = new UserSummaryDto
            {
                Id = user.Id,
                AvatarUrl = user.AvatarUrl,
                Friends = user.Friendships,
                IsOnline =  user.IsOnline,
                LastActive =  user.LastActive,
                UserName =  user.UserName,
            };
            
            // 3. Get friendList
            var friendsEntities = await _unitOfWork.FriendRepository.GetListFriendByUserIdAsync(Id);
            
            
            // 4. Logic mapping Friend Entity into FriendDto
            var friendDtoList = new List<FriendDto>();
            foreach (var item in friendsEntities)
            {
                // Xác định ai là bạn
                var friendUser = item.UserA == Id ? item.FriendB : item.FriendA;
            
                if(friendUser != null)
                {
                    friendDtoList.Add(new FriendDto
                    {
                        Id = friendUser.Id,
                        FriendUserName = friendUser.UserName,
                        FriendAvatarUrl = friendUser.AvatarUrl,
                        // Map thêm các trường khác nếu FriendDTO yêu cầu
                    });
                }
            }
            // 5. Gán vào kết quả trả về
            userDto.Friends = friendDtoList;

            return userDto;
        }
    }

}