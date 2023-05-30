using OnlineChatV2.WebApi.Hubs.EventManagement.Base;

namespace OnlineChatV2.WebApi.Hubs.EventManagement.EventsAndReceivers;

public struct ChatCreated : IEvent
{
    public long ChatId { get; set; }
    public string ChatName { get; set; }
    public IEnumerable<long> WhoAdded { get; set; }
}