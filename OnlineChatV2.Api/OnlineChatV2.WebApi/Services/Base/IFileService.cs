using OnlineChatV2.WebApi.Models;
using File = OnlineChatV2.WebApi.Models.File;

namespace OnlineChatV2.WebApi.Services.Base;

public interface IFileService
{
    Task<string> UploadAvatar(long id, IFormFile photo, string rootPath, AvatarType type);
    Task<string> UploadAvatar(long id, File photo, string rootPath, AvatarType type);
}