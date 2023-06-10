namespace OnlineChatV2.Domain;

public class ChatAvatar
{
    public long Id { get; set; }
    public long ChatId { get; set; }
    public Chat Chat { get; set; }
    public string AvatarUrl { get; set; }
}