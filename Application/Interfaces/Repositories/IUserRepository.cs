using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User> FindByEmailAsync(string email);
    Task<User> FindByResetTokenAsync(string tokenHash);
}