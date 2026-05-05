using BCrypt.Net;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BookTracker.Api.Models;
using Microsoft.Extensions.Configuration;

namespace BookTracker.Api.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;

    public AuthService(IConfiguration config)
    {
        _config = config;
        _jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured. Authentication cannot proceed.");
        _jwtIssuer = _config["Jwt:Issuer"] ?? "BookTracker";
        _jwtAudience = _config["Jwt:Audience"] ?? "BookTrackerUsers";

        if (_jwtKey.Length < 32)
        {
            throw new InvalidOperationException("JWT Key is too short. Minimum 32 characters required for HMAC-SHA256.");
        }
    }

    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool ComparePassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);

    public bool ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return regex.IsMatch(email);
    }

    public bool ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");
        return regex.IsMatch(password);
    }

    public string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7), // Production should use shorter lived tokens + refresh tokens
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
