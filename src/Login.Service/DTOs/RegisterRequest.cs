namespace Login.Service.DTOs;

internal sealed class RegisterRequest
{

    /*Neden init: DTO'lar oluşturulduktan sonra değiştirilmemeli.
      init ile sadece nesne oluşturulurken set edilebilir, sonra immutable kalır. 
      DTO’lar oluşturulduktan sonra değiştirilmeyecek veri taşıyıcıları olarak tasarlandı.*/
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}