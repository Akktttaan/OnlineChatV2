namespace OnlineChatV2.WebApi.Models;

public class CreateChatModel
{
    public long CreateById { get; set; }
    public long[] ChatUsersId { get; set; }
    public string ChatName { get; set; }
}