namespace OnlineChatV2.Domain;

public class UserAvatar
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public string AvatarUrl { get; set; }
}