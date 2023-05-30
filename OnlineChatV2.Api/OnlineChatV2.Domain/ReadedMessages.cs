namespace OnlineChatV2.Domain;

public class ReadMessage
{
    public ulong Id { get; set; }
    
    public ulong MessageId { get; set; }
    
    public long ReadById { get; set; }
    public User ReadBy { get; set; }
    
    public DateTime ReadDate { get; set; }
}