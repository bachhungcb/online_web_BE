namespace Application.Interfaces.Service;

public interface IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}