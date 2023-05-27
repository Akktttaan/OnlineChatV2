using OnlineChatV2.WebApi.Hubs.EventManagement.Base;

namespace OnlineChatV2.WebApi.Hubs.EventManagement.EventsAndReceivers;

public struct MessageSend : IEvent
{
    public long From { get; set; }
    public long To { get; set; }
    public string Message { get; set; }
}