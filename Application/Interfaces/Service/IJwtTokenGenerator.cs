using Domain.Entities;

namespace Application.Interfaces.Service;

public interface IJwtTokenGenerator
{
    public string GenerateToken(User user);
}