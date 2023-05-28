using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineChatV2.Domain;
using OnlineChatV2.WebApi.Hubs.EventManagement;
using OnlineChatV2.WebApi.Hubs.EventManagement.EventsAndReceivers;
using OnlineChatV2.WebApi.Utilities;

namespace OnlineChatV2.WebApi.Hubs;


public class ChatHub : BaseChatHub
{
    private readonly EventBus _eventBus;
    private ConcurrentDictionary<string, List<string>> _chats;

    public ChatHub([FromServices] EventBus eventBus)
    {
        _chats = new();
        _eventBus = eventBus;
    }

    public async Task EnterToChat(long chatId)
    {
        var user = GetUserFromContext(HttpContext);
        if (user == null) return;
        var id = CryptoUtilities.GetMd5String(user.Id, chatId);
        await Groups.AddToGroupAsync(Context.ConnectionId, id);
    }

    public async Task Send(long toId)
    {
        var user = GetUserFromContext(HttpContext);
        if (user == null)
            return;
        await Clients.All.SendAsync("Test");
        _eventBus.Invoke(new MessageSend() {From = user.Id, To = toId, Message = "Test"});
    }

    public void UserWriteEvent()
    {
        
    }
}