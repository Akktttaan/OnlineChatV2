using OnlineChatV2.Domain;

namespace OnlineChatV2.WebApi.Models.ActionContexts;

public class UserMoveActionContext : BaseActionContext
{
    public long TargetId { get; set; }
}