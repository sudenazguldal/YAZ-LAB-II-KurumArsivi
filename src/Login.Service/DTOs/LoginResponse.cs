namespace Login.Service.DTOs;

public sealed class LoginResponse
{

    /*Neden init: DTO'lar oluşturulduktan sonra değiştirilmemeli.
      init ile sadece nesne oluşturulurken set edilebilir, sonra immutable kalır. 
      DTO’lar oluşturulduktan sonra değiştirilmeyecek veri taşıyıcıları olarak tasarlandı.*/
    public string Token { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    private LoginResponse() { }

    public LoginResponse(string token, string username, string role)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty.");
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty.");

        Token = token;
        Username = username;
        Role = role;
    }

}