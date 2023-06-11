namespace OnlineChatV2.WebApi.Models;

public class ChatInfo
{
    public long ChatId { get; set; }
    public string ChatName { get; set; }
    public string? ChatDescription { get; set; }
    public ChatMember[] Members { get; set; }
    public long OwnerId { get; set; }
    public string? AvatarUrl { get; set; }
}

public class ChatMember
{
    public long UserId { get; set; }
    public string UserName { get; set; }
    public string? AvatarUrl { get; set; }
}