using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineChatV2.WebApi.Hubs.EventManagement;
using OnlineChatV2.WebApi.Hubs.EventManagement.EventsAndReceivers;

namespace OnlineChatV2.WebApi.Hubs;

public class NotifyHub : Hub
{
    public NotifyHub([FromServices] EventBus eventBus)
    {
        eventBus.Register(new NotifyHubReceiver(OnMessageSend));
    }

    private void OnMessageSend(MessageSend @event)
    {
        Console.WriteLine($"Event from ChatHub: From {@event.From} to {@event.To}. Message: {@event.Message}");
    }
}