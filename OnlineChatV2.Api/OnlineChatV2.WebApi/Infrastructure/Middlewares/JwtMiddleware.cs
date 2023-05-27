using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OnlineChatV2.WebApi.Services;
using OnlineChatV2.WebApi.Services.Base;
using OnlineChatV2.WebApi.Services.Implementation;

namespace OnlineChatV2.WebApi.Infrastructure.Middlewares;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context, [FromServices] IUserService authService)
    {
        var token = context.Request.Headers["UserToken"].FirstOrDefault();

        if (token != null)
        {
            await AttachUserToContext(context, authService, token);
        }

        await _next(context);
    }

    private async Task AttachUserToContext(HttpContext context, IUserService authService, string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["SecretKey"]);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
            }, out var validatedToken);

            var jwt = (JwtSecurityToken)validatedToken;
            var userId = int.Parse(jwt.Claims.First(x => x.Type == "id").Value);

            context.Items["User"] = await authService.GetUserById(userId);
        }
        catch 
        {
            
        }
    }
}