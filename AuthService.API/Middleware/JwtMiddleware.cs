using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.API.Middleware;

public class JwtMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private readonly IConfigurationSection _jwt = configuration.GetSection("Jwt");

    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last();

        if (token is not null)
            AttachUserToContext(context, token);

        await next(context);
    }

    private void AttachUserToContext(HttpContext context, string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt["Secret"]!));
            var handler = new JwtSecurityTokenHandler();

            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _jwt["Issuer"],
                ValidateAudience = true,
                ValidAudience = _jwt["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var jwt = (JwtSecurityToken)validatedToken;
            var userId = jwt.Claims.First(c => c.Type == "sub").Value;
            var role = jwt.Claims.First(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Value;

            context.Items["UserId"] = userId;
            context.Items["Role"] = role;
        }
        catch
        {
            // token inválido — segue sem autenticar
        }
    }
}