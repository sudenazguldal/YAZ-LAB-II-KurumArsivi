using Document.Service.Repositories;
using Document.Service.Services;
using Document.Service.Settings;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<MongoDbSettings>(options =>
{
    options.ConnectionString = Environment.GetEnvironmentVariable("MongoDB__ConnectionString")
                               ?? "mongodb://localhost:27017";
    options.DatabaseName = "DocumentDb";
    options.DocumentsCollection = "documents";
});

builder.Services.AddHttpClient("SearchService", client =>
{
    client.BaseAddress = new Uri(
        Environment.GetEnvironmentVariable("SearchService__Url")
        ?? "http://localhost:5002");
});

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();