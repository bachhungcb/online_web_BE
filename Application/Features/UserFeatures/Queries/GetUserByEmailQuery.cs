using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.UserFeatures.Queries;

public record GetUserByEmailQuery(string email) : IRequest<User>
{
    public class GetUserByEmailHandler : IRequestHandler<GetUserByEmailQuery, User>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetUserByEmailHandler(
            IApplicationDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<User> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            var Email = request.email;
            var user = await _context.Users.Where(u => u.Email == Email).FirstOrDefaultAsync();
            if (user == null) return null;
            return user;
        }
    }
}