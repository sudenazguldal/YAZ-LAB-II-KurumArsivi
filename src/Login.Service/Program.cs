using Login.Service.Services;
using Login.Service.Settings;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();



builder.Services.Configure<MongoDbSettings>(options =>
{
    options.ConnectionString = Environment.GetEnvironmentVariable("MongoDB__ConnectionString")
                               ?? "mongodb://localhost:27017";
    options.DatabaseName = "LoginDb";
    options.UsersCollection = "users";
});

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();


builder.Services.Configure<JwtSettings>(options =>
{
    options.Secret = Environment.GetEnvironmentVariable("Jwt__Secret") ?? string.Empty;
    options.Issuer = "KurumArsivi";
    options.Audience = "KurumArsivi";
    options.ExpiryHours = 8;
});

var app = builder.Build();

//admin seed
using (var scope = app.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    await authService.SeedAdminAsync();
}

app.MapControllers();
app.Run();