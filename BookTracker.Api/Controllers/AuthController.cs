using Microsoft.AspNetCore.Mvc;
using BookTracker.Api.Models;
using BookTracker.Api.Data;
using BookTracker.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace BookTracker.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[EnableRateLimiting("auth")]
public class AuthController : BaseApiController
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IAuthService authService, IConfiguration config)
    {
        _context = context;
        _authService = authService;
        _config = config;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var user = await _context.Users.FindAsync(UserId);
        if (user == null) return NotFound();
        
        if (!user.IsActive) return Unauthorized(new { message = "Your access to the archive has been suspended." });

        user.LastActiveAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new UserProfile 
        { 
            Id = user.Id, 
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
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
            PasswordHash = _authService.HashPassword(request.Password),
            Role = email == "curator@athenaeum.com" ? "Admin" : "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _authService.GenerateJwtToken(user);
        SetTokenCookie(token);

        return CreatedAtAction(nameof(Register), new { id = user.Id }, new AuthResponse 
        { 
            Token = token, // Keeping in body for legacy support if needed, but frontend should use cookie
            User = new UserProfile 
            { 
                Id = user.Id, 
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        });
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var email = request.Email.ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        bool isValid = false;
        try 
        {
            isValid = user != null && _authService.ComparePassword(request.Password, user.PasswordHash);
        }
        catch (Exception)
        {
            isValid = false;
        }

        if (!isValid)
            return Unauthorized(new { message = "Invalid email or password" });

        if (!user!.IsActive)
            return Unauthorized(new { message = "Your access to the archive has been suspended." });

        user.LastActiveAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = _authService.GenerateJwtToken(user!);
        SetTokenCookie(token);

        return Ok(new AuthResponse 
        { 
            Token = token,
            User = new UserProfile 
            { 
                Id = user!.Id, 
                Email = user!.Email,
                Role = user!.Role,
                IsActive = user!.IsActive,
                CreatedAt = user!.CreatedAt
            }
        });
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("athenaeum_auth");
        return NoContent();
    }

    private void SetTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Always true in production, assuming HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("athenaeum_auth", token, cookieOptions);
    }
}
