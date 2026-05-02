using BookTracker.Api.Models;

namespace BookTracker.Api.Services;

public interface IAuthService
{
    string HashPassword(string password);
    bool ComparePassword(string password, string hash);
    bool ValidateEmail(string email);
    bool ValidatePassword(string password);
    string GenerateJwtToken(User user);
}
