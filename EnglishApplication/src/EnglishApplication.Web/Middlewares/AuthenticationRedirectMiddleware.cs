using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace EnglishApplication.Web.Middlewares
{
    public class AuthenticationRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;

            // İzin verilen yollar kontrolü
            bool isAllowed =
                // 1. Statik dosyalara izin ver
                path.StartsWith("/libs") ||
                path.StartsWith("/images") ||
                path.StartsWith("/styles") ||
                path.StartsWith("/fonts") ||
                path.StartsWith("/favicon") ||
                path.Contains(".") ||

                // 2. Home sayfasına izin ver (sadece Home'a izin ver, kök dizine değil)
                path.Equals("/Home") ||

                // 3. Account sayfalarına izin ver ("/Account/" ile başlayan tüm yollar)
                path.StartsWith("/Account/") ||

                // 4. API ve swagger için izin ver
                path.StartsWith("/api/") ||
                path.StartsWith("/swagger");

            // Kullanıcı giriş durumuna göre yönlendirme
            if (context.User.Identity.IsAuthenticated)
            {
                // Giriş yapmış kullanıcı Home sayfasında ise ana sayfaya yönlendir
                if (path.Equals("/Home"))
                {
                    context.Response.Redirect("/");
                    return;
                }
            }
            else
            {
                // Giriş yapmamış kullanıcı izin verilen yollar dışında bir yere gitmek istiyorsa Home'a yönlendir
                if (!isAllowed)
                {
                    context.Response.Redirect("/Home");
                    return;
                }
            }

            await _next(context);
        }
    }

    // Extension metodu
    public static class AuthenticationRedirectMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationRedirect(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticationRedirectMiddleware>();
        }
    }
}
