using Api.Models;

namespace Api.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
