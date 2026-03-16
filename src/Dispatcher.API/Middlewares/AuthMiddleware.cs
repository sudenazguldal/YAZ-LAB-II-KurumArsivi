using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Threading.Tasks;

namespace Dispatcher.API.Middlewares
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _secretKey;

        public AuthMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _secretKey = configuration["Jwt__Secret"]
                         ?? throw new InvalidOperationException("JWT secret not configured");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // /api/login whitelist
            if (context.Request.Path.StartsWithSegments("/api/login"))
            {
                await _next(context);
                return;
            }
           

            // Token var mı?
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("No token provided.");
                return;
            }

            var token = context.Request.Headers["Authorization"].ToString();

            // "Bearer " formatında mı?
            
            if (!token.StartsWith("Bearer "))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid token format.");
                return;
            }


            // JWT imzasını doğrula
            var jwtToken = token.Substring(7); // "Bearer " kısmını at
            if (string.IsNullOrWhiteSpace(jwtToken))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Bearer token is empty.");
                return;
            }

            if (!ValidateToken(jwtToken))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid or expired token.");
                return;
            }
           
            await _next(context);
        } 

        private bool ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out _);

                return true;
            }
            catch
            {
                return false; // İmza geçersiz → 401
            }
        }
    }
}