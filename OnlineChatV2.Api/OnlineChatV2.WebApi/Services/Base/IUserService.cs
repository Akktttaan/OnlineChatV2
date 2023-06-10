using Microsoft.AspNetCore.Mvc;
using OnlineChatV2.Domain;
using OnlineChatV2.WebApi.Models;

namespace OnlineChatV2.WebApi.Services.Base;

public interface IUserService
{
    Task<AuthenticateResponse?> Auth(string nameOrEmail, string password);

    Task<AuthenticateResponse?> Register(RegisterDto dto);

    string GenerateJwt(User user);

    Task<User?> GetUserById(long id);

    Task PromoteToAdmin(long id);

    Task<UserViewModel[]> GetAllUsers();

    Task<DateTime> UpdateLastSeenTime(long userId);

    Task<UserInfo> GetUserInfo(long userId);
    Task UpdateAbout(long userId, string about);
    Task UploadPhoto(long userId, IFormFile photo, IWebHostEnvironment en);
}