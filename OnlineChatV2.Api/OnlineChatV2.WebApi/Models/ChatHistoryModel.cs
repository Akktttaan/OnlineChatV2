namespace OnlineChatV2.WebApi.Models;

public class ChatHistoryModel
{
    public long MessageId { get; set; }
    public SenderModel Sender { get; set; }
    public string MessageText { get; set; }
    public DateTime MessageDate { get; set; }
}

public class SenderModel
{
    public long UserId { get; set; }
    public string Username { get; set; }
    public string AvatarUrl { get; set; }
}