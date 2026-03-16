namespace Login.Service.DTOs;

public sealed class LoginResponse
{

    /*Neden init: DTO'lar oluşturulduktan sonra değiştirilmemeli.
      init ile sadece nesne oluşturulurken set edilebilir, sonra immutable kalır. 
      DTO’lar oluşturulduktan sonra değiştirilmeyecek veri taşıyıcıları olarak tasarlandı.*/
    public string Token { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
}