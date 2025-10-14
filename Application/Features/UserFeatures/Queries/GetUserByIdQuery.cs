using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.UserFeatures.Queries;

public record GetUserByIdQuery(Guid userId) : IRequest<User>
{


    public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, User>
    {
        private readonly IApplicationDbContext _context;

        public GetUserByIdHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var Id = request.userId;
            var user = await _context.Users.Where(a => a.Id == Id).FirstOrDefaultAsync();
            if (user == null) return null;
            return user;
        }
    }

}