using System.ComponentModel.DataAnnotations;

namespace OnlineChatV2.Domain;

public class ChatUser
{
    [Key]
    public long Id { get; set; }
    public long ChatId { get; set; }
    public Chat Chat { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
}