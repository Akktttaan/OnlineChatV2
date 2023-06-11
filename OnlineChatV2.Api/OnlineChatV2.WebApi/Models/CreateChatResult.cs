namespace OnlineChatV2.WebApi.Models;

public class CreateChatResult
{
    public long Id { get; set; }
    public IEnumerable<long> WhoAdded { get; set; }
    public string? Description { get; set; }
    public string? AvatarUrl { get; set; }
}