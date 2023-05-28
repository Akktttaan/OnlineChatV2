using Microsoft.AspNetCore.SignalR;
using OnlineChatV2.Domain;

namespace OnlineChatV2.WebApi.Hubs;

public class BaseChatHub : Hub
{
    #region User helper
    protected HttpContext? HttpContext => Context.GetHttpContext();
    protected User? GetUserFromContext(HttpContext? context)
    {
        return context?.Items["User"] as User;
    }
    #endregion
}