namespace MultiVendor.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(string userId, string email, IList<string> roles);
}
