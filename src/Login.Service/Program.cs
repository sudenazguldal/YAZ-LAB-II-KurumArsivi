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

builder.Services.Configure<JwtSettings>(options =>
{
    options.Secret = builder.Configuration["Jwt__Secret"] ?? string.Empty;
    options.Issuer = builder.Configuration["Jwt:Issuer"] ?? "KurumArsivi";
    options.Audience = builder.Configuration["Jwt:Audience"] ?? "KurumArsivi";
    options.ExpiryHours = int.TryParse(
        builder.Configuration["Jwt:ExpiryHours"], out var hours) ? hours : 8;
});

var app = builder.Build();

app.MapControllers();
app.Run();