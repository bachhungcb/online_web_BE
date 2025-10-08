using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.UserFeatures.Commands;

public class CreateUserCommand : IRequest<Guid>
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string PasswordHash { get; set; }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreateUserCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.UserName,
                PasswordHash = request.PasswordHash
            };
            _context.Users.Add(user);
            await _context.SaveChanges();
            return user.Id;
        }
    }
}