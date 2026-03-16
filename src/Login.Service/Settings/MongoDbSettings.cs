namespace Login.Service.Settings;

internal sealed class MongoDbSettings
{

    /*Neden burada set kaldı: IOptions<T> sistemi config'den okurken reflection ile set ediyor.
    private set veya init yaparsak framework hata verir, bu bir istisna.*/
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "LoginDb";
    public string UsersCollection { get; set; } = "users";
}