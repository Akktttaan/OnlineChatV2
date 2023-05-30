using OnlineChatV2.WebApi.Hubs.EventManagement.Base;

namespace OnlineChatV2.WebApi.Hubs.EventManagement.EventsAndReceivers;

public class ChatCreatedReceiver : IEventReceiver<ChatCreated>
{
    private readonly Action<ChatCreated> _action;

    public ChatCreatedReceiver(Action<ChatCreated> action)
    {
        _action = action;
    }

    public void OnEvent(ChatCreated @event)
    {
        _action?.Invoke(@event);
    }
}