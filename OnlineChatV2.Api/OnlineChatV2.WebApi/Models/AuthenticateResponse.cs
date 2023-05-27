using OnlineChatV2.Domain;

namespace OnlineChatV2.WebApi.Models;

public class AuthenticateResponse
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    
    public AuthenticateResponse(User user, string token)
    {
        Token = token;
        Id = user.Id;
        Username = user.Username;
        Email = user.Email;
    }
}