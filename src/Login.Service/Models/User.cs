using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Login.Service.Models;


/*Neden internal sealed: User modeli sadece Login.Service assembly'si içinde kullanılıyor.
 Dışarıya hiç çıkmıyor. sealed çünkü kalıtım almayı planlamıyoruz.*/
internal sealed class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    /*Neden Id public: MongoDB driver reflection ile bu property'e erişmek zorunda, 
     internal olursa MongoDB serialize/deserialize edemez. Bu bir framework kısıtlaması.*/

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("role")]
    public string Role { get; set; } = "user";
}