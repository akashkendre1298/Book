using Microsoft.AspNetCore.Mvc;
using BookTracker.Api.Services;
using BookTracker.Api.Models;
using BookTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _context;

    public AuthController(IAuthService authService, AppDbContext context)
    {
        _authService = authService;
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var email = request.Email.ToLowerInvariant();
        if (!_authService.ValidateEmail(email))
            return BadRequest(new { message = "Invalid email format" });

        if (!_authService.ValidatePassword(request.Password))
            return BadRequest(new { message = "Password too weak" });

        if (await _context.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { message = "Email already registered" });

        var user = new User
        {
            Email = email,
            PasswordHash = _authService.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _authService.GenerateJwtToken(user);

        return CreatedAtAction(nameof(Register), new AuthResponse
        {
            Token = token,
            User = new UserProfile { Id = user.Id, Email = user.Email }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var email = request.Email.ToLowerInvariant();
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user == null || !_authService.ComparePassword(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials" });

        var token = _authService.GenerateJwtToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            User = new UserProfile { Id = user.Id, Email = user.Email }
        });
    }
}
