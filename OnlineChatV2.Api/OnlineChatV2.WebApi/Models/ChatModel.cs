namespace OnlineChatV2.WebApi.Models;

public class ChatModel
{
    public long? Id { get; set; }
    public string Name { get; set; }
    public bool LastMessageFromSender { get; set; }
    public string LastMessageSenderName { get; set; }
    public string LastMessageText { get; set; }
    public DateTime? LastMessageDate { get; set; }
}