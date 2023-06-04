using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using OnlineChatV2.WebApi.Services.Base;

namespace OnlineChatV2.WebApi.Infrastructure;

public class AuthHubFilter : IHubFilter
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public AuthHubFilter(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        await TokenHandling(context.Context);
        await next(context);
    }
        
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext, 
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            await TokenHandling(invocationContext.Context);
            return await next(invocationContext);   
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не удалось вызвать метод {invocationContext.HubMethodName}: {ex.Message}");
            throw;
        }
    }

    private async Task TokenHandling(HubCallerContext context)
    {
        var httpContext = context.GetHttpContext();
        var accessToken = httpContext.Request.Query["access_token"];
        if (!string.IsNullOrEmpty(accessToken))
            await AttachUserToContext(httpContext, _userService, accessToken);
        else
            throw new AuthenticationException("Unauhorized");
    }
    
    private async Task AttachUserToContext(HttpContext context, IUserService authService, string token)
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

            var user = await authService.GetUserById(userId);
            if (user == null)
                throw new AuthenticationException("Unauthorized");
            context.Items["User"] = user;
        }
}