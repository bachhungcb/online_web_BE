using Application.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.UserFeatures.Queries;

// 1. Sửa kiểu trả về thành List<Guid> cho đơn giản
// Thêm tham số ExcludeUserId (có thể null)
public record GetListUserIdQuery(Guid? ExcludeUserId = null) : IRequest<List<Guid>>;

public class GetListUserIdQueryHandler : IRequestHandler<GetListUserIdQuery, List<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetListUserIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Guid>> Handle(
        GetListUserIdQuery query,
        CancellationToken cancellationToken)
    {
        
        // 1. Tối ưu Query: Chỉ select cột Id ngay từ đầu (Projection)
        // Lưu ý: Nếu data lớn, BẮT BUỘC phải dùng Phân trang (Pagination) ở đây
        var userIds =  _unitOfWork.UserRepository.GetAllAsQueryable();
           
        if (query.ExcludeUserId.HasValue)
        {
            userIds = userIds.Where(u => u.Id != query.ExcludeUserId.Value);
        }
        
        // 2. Không cần check null, trả về luôn
        return await userIds.Select(u => u.Id).ToListAsync(cancellationToken);;
    }
}