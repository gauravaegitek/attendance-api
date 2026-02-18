// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using Microsoft.IdentityModel.Tokens;
// using attendance_api.Models;

// namespace attendance_api.Services
// {
//     public interface IJwtService
//     {
//         string GenerateToken(User user);
//         ClaimsPrincipal? ValidateToken(string token);
//     }

//     public class JwtService : IJwtService
//     {
//         private readonly IConfiguration _configuration;

//         public JwtService(IConfiguration configuration)
//         {
//             _configuration = configuration;
//         }

//         public string GenerateToken(User user)
//         {
//             var secretKey = _configuration["JwtSettings:SecretKey"] 
//                 ?? throw new InvalidOperationException("JWT Secret Key not configured");
//             var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
//             var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//             var claims = new[]
//             {
//                 new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
//                 new Claim(ClaimTypes.Role, user.Role)
//             };

//             var expiryHours = int.Parse(_configuration["JwtSettings:ExpiryInHours"] ?? "24");

//             var token = new JwtSecurityToken(
//                 issuer: _configuration["JwtSettings:Issuer"],
//                 audience: _configuration["JwtSettings:Audience"],
//                 claims: claims,
//                 expires: DateTime.UtcNow.AddHours(expiryHours),
//                 signingCredentials: credentials
//             );

//             return new JwtSecurityTokenHandler().WriteToken(token);
//         }

//         public ClaimsPrincipal? ValidateToken(string token)
//         {
//             try
//             {
//                 var secretKey = _configuration["JwtSettings:SecretKey"] 
//                     ?? throw new InvalidOperationException("JWT Secret Key not configured");
//                 var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

//                 var tokenHandler = new JwtSecurityTokenHandler();
//                 var validationParameters = new TokenValidationParameters
//                 {
//                     ValidateIssuer = true,
//                     ValidateAudience = true,
//                     ValidateLifetime = true,
//                     ValidateIssuerSigningKey = true,
//                     ValidIssuer = _configuration["JwtSettings:Issuer"],
//                     ValidAudience = _configuration["JwtSettings:Audience"],
//                     IssuerSigningKey = key,
//                     ClockSkew = TimeSpan.Zero
//                 };

//                 var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
//                 return principal;
//             }
//             catch
//             {
//                 return null;
//             }
//         }
//     }
// }












using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using attendance_api.Models;

namespace attendance_api.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"]
                ?? throw new InvalidOperationException("JWT Secret Key not configured (JwtSettings:SecretKey)");

            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var expiryHours = int.TryParse(_configuration["JwtSettings:ExpiryInHours"], out var h) ? h : 24;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // IMPORTANT: ClaimTypes.Role is what ASP.NET uses for [Authorize(Roles="...")]
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Role, user.Role ?? string.Empty),

                // Optional but useful
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var secretKey = _configuration["JwtSettings:SecretKey"]
                    ?? throw new InvalidOperationException("JWT Secret Key not configured (JwtSettings:SecretKey)");

                var issuer = _configuration["JwtSettings:Issuer"];
                var audience = _configuration["JwtSettings:Audience"];

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                    ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = key,

                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}
