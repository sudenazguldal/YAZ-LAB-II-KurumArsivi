namespace Search.Service.Settings;

internal sealed class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "SearchDb";
    public string CollectionName { get; set; } = "documents";
}