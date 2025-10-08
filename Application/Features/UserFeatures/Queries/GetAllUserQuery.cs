using Application.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.UserFeatures.Queries;

public record GetAllUserQuery(int PageNumber, int PageSize) : IRequest<IEnumerable<User>>
{
    public class GetAllUserQueryHandler : IRequestHandler<GetAllUserQuery, IEnumerable<User>>
    {
        private readonly IApplicationDbContext _context;
        
        public GetAllUserQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> Handle(GetAllUserQuery query, CancellationToken cancellationToken)
        {
            int pageSize = query.PageSize;
            int pageNumber = query.PageNumber;
            var userList = await _context.Users
                                         .Skip((pageNumber-1) * pageSize)
                                         .Take(pageSize)
                                         .ToListAsync(cancellationToken: cancellationToken);
            if (userList == null)
            {
                return null;
            }
            return userList.AsReadOnly();
        }
    }
}