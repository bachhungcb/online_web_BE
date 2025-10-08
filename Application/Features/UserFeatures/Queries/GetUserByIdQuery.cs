using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.UserFeatures.Queries;

public class GetUserByIdQuery : IRequest<User>
{
    public Guid Id { get; set; }

    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, User>
    {
        private readonly IApplicationDbContext _context;

        public GetUserByIdHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.Where(a => a.Id == request.Id).FirstOrDefaultAsync();
            if (user == null) return null;
            return user;
        }
    }

}