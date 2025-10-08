using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.UserFeatures.Commands;

public class UpdateUserCommand : IRequest<Guid>
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public UpdateUserCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.Where(u => u.Id == request.Id).FirstOrDefaultAsync(cancellationToken);
            if (user == null)
            {
                return Guid.Empty;
            }
            else
            {
                user.FullName = request.FullName;
                user.Email = request.Email;
                user.UserName = request.UserName;
                await _context.SaveChanges();
                return user.Id;
            }
        }
    }
}