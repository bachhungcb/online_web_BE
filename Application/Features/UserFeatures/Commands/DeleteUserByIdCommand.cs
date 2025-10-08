using System.Diagnostics;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.UserFeatures.Commands;

public class DeleteUserByIdCommand : IRequest<Guid>
{
    public Guid Id { get; set; }

    public class DeleteUserByIdCommandHandler : IRequestHandler<DeleteUserByIdCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public DeleteUserByIdCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(DeleteUserByIdCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.Where(u => u.Id == request.Id).FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (user == null) return Guid.Empty;
            _context.Users.Remove(user);
            await _context.SaveChanges();
            return user.Id;
        }
    }
}