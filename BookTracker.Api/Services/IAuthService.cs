namespace BookTracker.Api.Services;

public interface IAuthService
{
    string HashPassword(string password);
    bool ValidateEmail(string email);
    bool ValidatePassword(string password);
}
