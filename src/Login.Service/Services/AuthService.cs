using DnsClient;
using Login.Service.DTOs;
using Login.Service.Models;
using Login.Service.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Login.Service.Services;

internal sealed class AuthService : IAuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IOptions<MongoDbSettings> mongoSettings, IOptions<JwtSettings> jwtSettings)
    {
        var client = new MongoClient(mongoSettings.Value.ConnectionString);
        var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
        _users = database.GetCollection<User>(mongoSettings.Value.UsersCollection);
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var existing = await _users
            .Find(u => u.Username == request.Username)
            .FirstOrDefaultAsync();

        if (existing != null)
            return false;

        var user = new User
        {
            Username = request.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "user"
        };

        await _users.InsertOneAsync(user);
        return true;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _users
            .Find(u => u.Username == request.Username)
            .FirstOrDefaultAsync();

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var token = GenerateJwtToken(user);

        return new LoginResponse
        {
            Token = token,
            Username = user.Username
        };
    }

    private string GenerateJwtToken(User user)
    {
        if (string.IsNullOrWhiteSpace(_jwtSettings.Secret))
            throw new InvalidOperationException("JWT secret not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpiryHours),
                signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
