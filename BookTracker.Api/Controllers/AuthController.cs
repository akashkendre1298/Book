using Microsoft.AspNetCore.Mvc;
using BookTracker.Api.Models;
using BookTracker.Api.Data;
using BookTracker.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : BaseApiController
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;

    public AuthController(AppDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> GetMe()
    {
        var user = await _context.Users.FindAsync(UserId);
        if (user == null) return NotFound();

        return Ok(new UserProfile 
        { 
            Id = user.Id, 
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        });
    }

    /// <summary>
    /// Registers a new user in the system.
    /// </summary>
    /// <param name="request">Registration details (Email and Password)</param>
    /// <returns>Auth token and user profile</returns>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="409">Email already registered</response>
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
            User = new UserProfile 
            { 
                Id = user.Id, 
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            }
        });
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">Login credentials (Email and Password)</param>
    /// <returns>Auth token and user profile</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            // Log error here in a real app. Invalid hash format can cause BCrypt to throw.
            isValid = false;
        }

        if (!isValid)
            return Unauthorized(new { message = "Invalid email or password" });

        var token = _authService.GenerateJwtToken(user!);
        return Ok(new AuthResponse 
        { 
            Token = token,
            User = new UserProfile 
            { 
                Id = user!.Id, 
                Email = user!.Email,
                Role = user!.Role,
                CreatedAt = user!.CreatedAt
            }
        });
    }

    /// <summary>
    /// Terminates the current session.
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        return NoContent();
    }
}
