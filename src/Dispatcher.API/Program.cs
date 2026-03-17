using Dispatcher.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
// HttpClient factory kaydı — RoutingMiddleware bunu kullanacak
builder.Services.AddHttpClient();

var app = builder.Build();

//midddleware'i pipeline'a ekliyoruz. Bu, her isteğin önce AuthMiddleware tarafından işleneceği anlamına gelir.
//önce auth kontrolü yapacağız, sonra yönlendirme yapacağız. Bu sırayla ekliyoruz.
app.UseMiddleware<AuthMiddleware>();
app.UseMiddleware<RoutingMiddleware>();

// Test amaçlı basit bir sonuç dönüyoruz.
app.MapGet("/", () => "Dispatcher Gateway is running.");

app.Run();