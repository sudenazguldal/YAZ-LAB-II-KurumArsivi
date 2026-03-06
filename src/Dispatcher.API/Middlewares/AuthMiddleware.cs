using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Dispatcher.API.Middlewares
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            // _next "eğer sorun yoksa isteği içerideki servislere gönder" komutudur.
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kural 1: Kullanıcı Login yapmaya  çalışıyorsa ona kimlik sorma. bırak geçsin.
            if (context.Request.Path.StartsWithSegments("/api/login"))
            {
                await _next(context);
                return;
            }

            // Kural 2: Diğer tüm istekler için "Authorization" (Yetki) başlığı var mı kontrol et.
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                // Kimlik yoksa 401 fırlat ve işlemi burada bitir. İçeri giremez.
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("CRITICAL ERROR: No token provided. Access Denied.");
                return;
            }

            // Eğer kimlik varsa, şimdilik geçmesine izin ver (İleride burada JWT doğrulaması yapacağız).
            await _next(context);
        }
    }
}