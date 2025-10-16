using Application.Interfaces;
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
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public CreateUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _passwordHasher = passwordHasher;
            _context = context;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var hashedPassword = _passwordHasher.HashPassword(request.Password);
            var createdAt = DateTime.UtcNow;
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
            _context.Users.Add(user);
            await _context.SaveChanges();
            return user.Id;
        }
    }
}