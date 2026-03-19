using Login.Service.DTOs;

namespace Login.Service.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
    Task SeedAdminAsync();
    Task<List<UserDto>> GetAllUsersAsync();
}