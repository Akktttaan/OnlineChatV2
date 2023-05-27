using OnlineChatV2.WebApi.Hubs.EventManagement.Base;

namespace OnlineChatV2.WebApi.Hubs.EventManagement.EventsAndReceivers;

public class NotifyHubReceiver : IEventReceiver<MessageSend>
{
    private readonly Action<MessageSend> _handler;
    public NotifyHubReceiver(Action<MessageSend> handler)
    {
        _handler = handler;
    }
    public void OnEvent(MessageSend @event)
    {
        _handler.Invoke(@event);
    }
}