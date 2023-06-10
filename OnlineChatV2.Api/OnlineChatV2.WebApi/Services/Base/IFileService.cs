using OnlineChatV2.WebApi.Models;

namespace OnlineChatV2.WebApi.Services.Base;

public interface IFileService
{
    Task<string> UploadAvatar(long id, IFormFile photo, string rootPath, AvatarType type);
}