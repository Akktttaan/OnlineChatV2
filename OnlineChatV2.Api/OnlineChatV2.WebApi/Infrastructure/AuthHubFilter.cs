using System.IdentityModel.Tokens.Jwt;
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
    
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext, 
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var context = invocationContext.Hub.Context.GetHttpContext();
        try
        {
            if(context == null)
                return await next(invocationContext);   
            
            var accessToken = context.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(accessToken))
                await AttachUserToContext(context, _userService, accessToken);

            return await next(invocationContext);   
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не удалось вызвать метод {invocationContext.HubMethodName}: {ex.Message}");
            throw;
        }
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
        catch {}
    }
}