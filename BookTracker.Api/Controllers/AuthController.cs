using Microsoft.AspNetCore.Mvc;
using BookTracker.Api.Models;
using BookTracker.Api.Data;
using BookTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;

    public AuthController(AppDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var email = request.Email.ToLowerInvariant();
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
        return CreatedAtAction(nameof(Register), new { id = user.Id }, new AuthResponse 
        { 
            Token = token,
            User = new UserProfile { Id = user.Id, Email = user.Email }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var email = request.Email.ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !_authService.ComparePassword(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password" });

        var token = _authService.GenerateJwtToken(user);
        return Ok(new AuthResponse 
        { 
            Token = token,
            User = new UserProfile { Id = user.Id, Email = user.Email }
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return NoContent();
    }
}
