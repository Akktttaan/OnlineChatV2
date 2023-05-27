namespace OnlineChatV2.WebApi.Models;

public class UserViewModel
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public bool IsAdmin { get; set; }
}