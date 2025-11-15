namespace Application.Interfaces.Service;

public interface IToken
{
    string GenerateSafeRandomToken();

    string HashToken(string token);
}