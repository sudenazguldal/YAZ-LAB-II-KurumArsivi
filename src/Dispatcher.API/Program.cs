using Dispatcher.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//midddleware'i pipeline'a ekliyoruz. Bu, her isteğin önce AuthMiddleware tarafından işleneceği anlamına gelir.
app.UseMiddleware<AuthMiddleware>();

// Test amaçlı basit bir sonuç dönüyoruz.
app.MapGet("/", () => "Dispatcher Gateway is running.");

app.Run();