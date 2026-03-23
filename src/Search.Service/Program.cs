using Search.Service.Services;
using Search.Service.Settings;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.Configure<MongoDbSettings>(options =>
{
    options.ConnectionString = Environment.GetEnvironmentVariable("MongoDB__ConnectionString")
                               ?? "mongodb://localhost:27017";
    options.DatabaseName = "SearchDb";
    options.CollectionName = "documents";
});

builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();