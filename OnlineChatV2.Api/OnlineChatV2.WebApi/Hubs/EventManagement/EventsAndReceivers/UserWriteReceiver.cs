using OnlineChatV2.WebApi.Hubs.EventManagement.Base;

namespace OnlineChatV2.WebApi.Hubs.EventManagement.EventsAndReceivers;

public class UserWriteReceiver : IEventReceiver<UserWrite>
{
    private readonly Action<UserWrite> _onEventAction;
    public UserWriteReceiver(Action<UserWrite> onEventAction)
    {
        _onEventAction = onEventAction;
    }
    public void OnEvent(UserWrite @event)
    {
        _onEventAction.Invoke(@event);
    }
}