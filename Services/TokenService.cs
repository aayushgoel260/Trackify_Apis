using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TrackifyApis.Models;

namespace TrackifyApis.Services
{
    public class TokenService
    {

        private readonly JwtSettings _jwtSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenService(IOptions<JwtSettings> jwtSettings, IHttpContextAccessor httpContextAccessor)
        {
            _jwtSettings = jwtSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }


        public string GenerateToken(string emailId)
        {

            var request = _httpContextAccessor.HttpContext?.Request;
            var userAgent = request?.Headers["User-Agent"].ToString()?.ToLower();

            var referer = request?.Headers["Referer"].ToString()?.ToLower();
            var origin = request?.Headers["Origin"].ToString()?.ToLower();

            string source = "unknown";


            if (!string.IsNullOrEmpty(referer) && referer.Contains("swagger"))
                source = "swagger-ui";
            else if (!string.IsNullOrEmpty(userAgent) && userAgent.Contains("postmanruntime"))
                source = "postman";
            else if (!string.IsNullOrEmpty(userAgent) && userAgent.Contains("console-app-client"))
                source = "console-app";
            else
                source = "web-browser";


            var claims = new[]
            {
                new Claim(ClaimTypes.Name, emailId),
                new Claim("source", source)


            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GetTokenSource(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal.FindFirst("source")?.Value;
            }
            catch
            {
                return null;
            }
        }


    }
}
