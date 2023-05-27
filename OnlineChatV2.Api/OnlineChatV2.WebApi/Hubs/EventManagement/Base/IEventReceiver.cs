namespace OnlineChatV2.WebApi.Hubs.EventManagement.Base;

public interface IBaseEventReceiver
{
    
}

public interface IEventReceiver<in T> : IBaseEventReceiver where T : struct, IEvent
{
    void OnEvent(T @event);
}