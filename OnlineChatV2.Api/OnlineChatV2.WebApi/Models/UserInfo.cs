namespace OnlineChatV2.WebApi.Models;

public class UserInfo
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string About { get; set; }
    public DateTime LastSeen { get; set; }
    public string AvatarUrl { get; set; }
    public IEnumerable<string> Avatars { get; set; }
}