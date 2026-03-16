using Login.Service.Services;
using Login.Service.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(options =>
{
    options.ConnectionString = builder.Configuration["MongoDB__ConnectionString"]
                               ?? "mongodb://localhost:27017";
    options.DatabaseName = "LoginDb";
    options.UsersCollection = "users";
});

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();