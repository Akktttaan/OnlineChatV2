using System.ComponentModel.DataAnnotations;

namespace OnlineChatV2.Domain;

public class Chat
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; }
    public ICollection<ChatUser> ChatUsers { get; set; }
    public ICollection<Message> Messages { get; set; }
    public long OwnerId { get; set; }
    public User Owner { get; set; }
    public string? Description { get; set; }
    public string? CurrentAvatar { get; set; }
    public ICollection<ChatAvatar> Avatars { get; set; }
}