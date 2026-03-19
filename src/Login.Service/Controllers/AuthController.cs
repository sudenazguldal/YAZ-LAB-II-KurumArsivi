using Login.Service.DTOs;
using Login.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace Login.Service.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Username and password are required" });

        var response = await _authService.LoginAsync(request);

        if (response == null)
            return Unauthorized(new { message = "Invalid username or password" });

        return Ok(response);
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Sadece admin ekleyebilir
        var role = HttpContext.Request.Headers["X-User-Role"].ToString();
        Console.WriteLine($"X-User-Role header: '{role}'"); // debug
        if (role != "admin")
            return StatusCode(403, new { message = "Only admins can register users" });

        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Username and password are required" });

        var success = await _authService.RegisterAsync(request);

        if (!success)
            return Conflict(new { message = "Username already exists" });

        return StatusCode(201, new { message = "User registered successfully" });
    }

    // Kullanıcı listesi — sadece admin
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var role = HttpContext.Request.Headers["X-User-Role"].ToString();
        if (role != "admin")
            return StatusCode(403, new { message = "Only admins can view users" });

        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }
}