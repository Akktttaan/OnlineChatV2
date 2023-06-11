using Microsoft.VisualBasic;
using OnlineChatV2.Domain;
using OnlineChatV2.WebApi.Models;
using OnlineChatV2.WebApi.Services.Base;
using OnlineChatV2.WebApi.Utilities;
using File = OnlineChatV2.WebApi.Models.File;
using FileIO = System.IO.File;

namespace OnlineChatV2.WebApi.Services.Implementation;

public class FileService : IFileService
{
    private static readonly string[] _availableExtensions = { ".jpeg", ".jpg", ".png" };
    public async Task<string> UploadAvatar(long id, IFormFile photo, string rootPath, AvatarType type)
    {
        var guid = id + "-" + Guid.NewGuid();
        var extension = Path.GetExtension(photo.FileName.Trim('"'));
        if (!_availableExtensions.Contains(extension))
            throw new ArgumentException(
                $"Фото должно быть в одном из следующих форматов: {Strings.Join(_availableExtensions, "\n")}");
        var fileName = guid + extension;
        var avatarPath = Path.Combine(type.GetEnumInfo(), fileName);
        var fullPath = Path.Combine(rootPath, avatarPath);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await photo.CopyToAsync(stream);
        return avatarPath.Replace('\\', '/');
    }
    
    public async Task<string> UploadAvatar(long id, File photo, string rootPath, AvatarType type)
    {
        var guid = id + "-" + Guid.NewGuid();
        var extension = Path.GetExtension(photo.Name.Trim('"'));
        if (!_availableExtensions.Contains(extension))
            throw new ArgumentException(
                $"Фото должно быть в одном из следующих форматов: {Strings.Join(_availableExtensions, "\n")}");
        var fileName = guid + extension;
        var avatarPath = Path.Combine(type.GetEnumInfo(), fileName);
        var fullPath = Path.Combine(rootPath, avatarPath);
        var bytes = Convert.FromBase64String(photo.Data);
        await FileIO.WriteAllBytesAsync(fullPath, bytes);
        return avatarPath.Replace('\\', '/');
    }
}