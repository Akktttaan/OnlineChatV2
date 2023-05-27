using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnlineChatV2.Domain;

namespace OnlineChatV2.WebApi.Infrastructure;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly List<string> _roles = new();

    public AuthorizeAttribute(params string[] roleNames)
    {
        _roles.AddRange(roleNames);
    }

    public AuthorizeAttribute()
    {
        
    }
    
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = (User?)context.HttpContext.Items["User"];

        if (user == null)
        {
            context.Result = new JsonResult(new { message = "Unauthorized" })
                { StatusCode = StatusCodes.Status401Unauthorized };
            return;
        }
        
        var forbiddenResult = new JsonResult(new { message = "Forbidden" })
            { StatusCode = StatusCodes.Status403Forbidden };

        if (_roles.Count == 0) return;
        if (user.UserRoles.Count != 0)
        {
            var forbidden = true;
            foreach (var role in user.UserRoles)
            {
                if (_roles.Contains(role.Role.Name) && forbidden)
                {
                    forbidden = !forbidden;
                }
            }

            if (!forbidden) return;
            context.Result = forbiddenResult;
            return;
        }

        context.Result = forbiddenResult;
    }
}