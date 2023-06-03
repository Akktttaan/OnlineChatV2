namespace OnlineChatV2.Domain;

public class ReadMessage
{
    public long Id { get; set; }
    
    public long MessageId { get; set; }
    
    public long ReadById { get; set; }
    public User ReadBy { get; set; }
    
    public DateTime ReadDate { get; set; }
}