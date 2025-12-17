
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TrackifyApis.Middlewares
{
    public class UserAgentDetectionMiddleware
    {

        private readonly RequestDelegate _next;

        public UserAgentDetectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Console.WriteLine("Middleware trigerrred");
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var referer = context.Request.Headers["Referer"].ToString();
            var path = context.Request.Path.ToString();

            Console.WriteLine($"Raw User-Agent: {userAgent}");
            Console.WriteLine($"Request Path: {path}");
            Console.WriteLine($"Referer: {referer}");

            if (referer.Contains("/swagger", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Request from Swagger UI");
            }
            else if (userAgent.Contains("Postman", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Request from Postman");
            }
            else if (userAgent.Contains("console-app-client") || userAgent.Contains("ConsoleApp"))
            {
                Console.WriteLine("Request from Console App or HttpClient");
            }
            else
            {
                Console.WriteLine("Request from unknown source");
            }

            await _next(context);
        }
    }
}