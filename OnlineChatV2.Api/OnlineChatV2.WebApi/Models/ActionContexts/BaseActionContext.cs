using OnlineChatV2.Domain;

namespace OnlineChatV2.WebApi.Models.ActionContexts;

public class BaseActionContext
{
    public long ChatId { get; set; }
    public User Invoker { get; set; }
}