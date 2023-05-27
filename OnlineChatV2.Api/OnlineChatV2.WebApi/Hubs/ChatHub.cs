using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineChatV2.Domain;
using OnlineChatV2.WebApi.Hubs.EventManagement;
using OnlineChatV2.WebApi.Hubs.EventManagement.EventsAndReceivers;

namespace OnlineChatV2.WebApi.Hubs;


public class ChatHub : Hub
{
    private readonly EventBus _eventBus;

    public ChatHub([FromServices] EventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task Send(long toId)
    {
        var user = GetUserFromContext(HttpContext);
        if (user == null)
            return;
        await Clients.All.SendAsync("Test");
        _eventBus.Invoke(new MessageSend() {From = user.Id, To = toId, Message = "Test"});
    }

    #region User helper
    private HttpContext? HttpContext => Context.GetHttpContext();
    private User? GetUserFromContext(HttpContext? context)
    {
        return context?.Items["User"] as User;
    }
    #endregion
}