using System.ComponentModel.DataAnnotations;

namespace OnlineChatV2.Domain;

public class Chat
{
    [Key]
    public long Id { get; set; }
    public string Name { get; set; }
    public ICollection<ChatUser> ChatUsers { get; set; }
    public ICollection<Message> Messages { get; set; }
}