namespace OnlineChatV2.Domain;

public class UserContact
{
    public long Id { get; set; }
    public long ContactOwnerId { get; set; }
    public User ContactOwner { get; set; }
    public long ContactId { get; set; }
    public User Contact { get; set; }
}