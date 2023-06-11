namespace OnlineChatV2.WebApi.Models;

public class PushNotifyModel
{
    public long ChatId { get; set; }
    public string MessageText { get; set; }
    public SenderModel Sender { get; set; }
    public DateTime MessageDate { get; set; }
    public string? ChatName { get; set; }
    public string? AvatarUrl { get; set; }
}