using OnlineChatV2.Domain;
using OnlineChatV2.WebApi.Models;

namespace OnlineChatV2.WebApi.Services.Base;

public interface IUserService
{
    Task<AuthenticateResponse?> Auth(string nameOrEmail, string password);

    Task<AuthenticateResponse?> Register(string username, string password, string email);

    string GenerateJwt(User user);

    Task<User?> GetUserById(long id);

    Task PromoteToAdmin(long id);

    Task<UserViewModel[]> GetAllUsers();
}