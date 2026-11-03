using Microsoft.AspNetCore.Mvc;
// Authorization = for [Authorize] attribute
using Microsoft.AspNetCore.Authorization;
using OroFashionAPI.DTOs;
using OroFashionAPI.Services;
// ClaimTypes = for reading userId from JWT token
using System.Security.Claims;

namespace OroFashionAPI.Controllers;

// [ApiController] = enables automatic model validation and JSON binding
// [Route("api/[controller]")] = URL = api/auth (class name minus "Controller")
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // ControllerBase = base class for API controllers
    // provides Ok(), BadRequest(), Unauthorized(), NotFound() etc.
    private readonly AuthService _authService;
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    // [FromBody] = read JSON from request body and map to RegisterDTO
    // IActionResult = can return any HTTP status (200, 400, 401...)
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        if(string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password) ||
            string.IsNullOrWhiteSpace(dto.FullName))
            return BadRequest(new {message = "Name, email and password are required"});

            var (success, message, data) = await _authService.RegisterAsync(dto);

            if(!success)
                return BadRequest(new {message});
            return Ok(new {message, data});
    }
    [HttpPost("login")]
    public async Task<IActionResult> login([FromBody] LoginDTO dto)
    {
        if(string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new {message = "Email and password are required."});

            var (success, message, data) = await _authService.LoginAsync(dto);

            if(!success)
                return BadRequest(new {message});
            return Ok(new {message, data});
    }
    // PUT api/auth/profile
    // [Authorize] = must include valid JWT token
    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO dto)
    {
        // User = ClaimsPrincipal — ASP.NET reads JWT and populates this
        // FindFirst(Sub) = get the userId from token claims
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;
        
        if(userId == null)
            return Unauthorized();

        var updated = await _authService.UpdateProfileAsync(userId, dto);
        if(!updated)
            return NotFound(new {message = "User not found"});
        return Ok(new {message = "Profile updated."});
    }
    [HttpGet("users")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        var result = users.Select(u => new
        {
            u.Id,
            u.FullName,
            u.Email,
            u.Phone,
            u.Role,
            u.CreateAt,
            u.LastLoginAt
        });
        return Ok(result);
    }
}
