using Application.Features.VideoCallFeatures.Command;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Service;
using Domain.Entities;
using MediatR;

namespace Application.Features.UserFeatures.Commands;

public record CreateUserCommand(
    string UserName,
    string Email,
    string FullName,
    string Password,
    string AvatarUrl,
    string Bio,
    string Phone) : IRequest<Guid>
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMediator _mediator;
        public CreateUserCommandHandler(
            IUnitOfWork unitOfWork, 
            IPasswordHasher passwordHasher,
            IMediator mediator)
        {
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // 1. Create hash password
            var hashedPassword = _passwordHasher.HashPassword(request.Password);
            var createdAt = DateTime.UtcNow;
            
            // 2. Create Entity
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.UserName,
                PasswordHash = hashedPassword,
                AvatarUrl = request.AvatarUrl,
                Bio = request.Bio,
                Phone = request.Phone,
                CreatedAt = createdAt,
            };
            
            // 3. Add to Repository
            _unitOfWork.UserRepository.Add(user);
            
            // 4. Save to DB
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            try
            {
                await _mediator.Send(new UpsertStreamUserCommand(
                    user.Id,
                    user.UserName
                    ), cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi đồng bộ Stream User: {ex.Message}");
            }
            // 5. Return Id
            return user.Id;
        }
    }
}