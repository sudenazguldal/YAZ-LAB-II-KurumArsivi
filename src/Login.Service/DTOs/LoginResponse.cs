namespace Login.Service.DTOs;

internal sealed class LoginResponse
{

    /*Neden init: DTO'lar oluşturulduktan sonra değiştirilmemeli.
     init ile sadece nesne oluşturulurken set edilebilir, sonra immutable kalır. Bu OOP'un encapsulation prensibi.*/
    public string Token { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
}