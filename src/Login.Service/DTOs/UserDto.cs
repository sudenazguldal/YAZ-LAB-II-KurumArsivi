namespace Login.Service.DTOs;

public sealed class UserDto
{
    public string Username { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;

    private UserDto() { }

    public UserDto(string username, string role)
    {
        Username = username;
        Role = role;
    }
}