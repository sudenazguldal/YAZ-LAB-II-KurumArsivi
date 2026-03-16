using Login.Service.DTOs;

namespace Login.Service.Services;

internal interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
}