using Dispatcher.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
// HttpClient factory kaydı — RoutingMiddleware bunu kullanacak
builder.Services.AddHttpClient();

//frontend için CORS politikası ekliyoruz. Bu, farklı origin'lerden gelen isteklerin kabul edilmesini sağlar.
//Geliştirme aşamasında tüm origin'lere izin veriyoruz,
//ancak üretim ortamında bunu daha kısıtlı hale getirmek isteyebilirsiniz.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

//cors en başa
app.UseCors();
//midddleware'i pipeline'a ekliyoruz. Bu, her isteğin önce AuthMiddleware tarafından işleneceği anlamına gelir.
//önce auth kontrolü yapacağız, sonra yönlendirme yapacağız. Bu sırayla ekliyoruz.
app.UseMiddleware<AuthMiddleware>();
app.UseMiddleware<RoutingMiddleware>();

// Test amaçlı basit bir sonuç dönüyoruz.
app.MapGet("/", () => "Dispatcher Gateway is running.");

app.Run();