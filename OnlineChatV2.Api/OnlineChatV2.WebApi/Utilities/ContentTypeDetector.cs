using OnlineChatV2.Domain.Enums;

namespace OnlineChatV2.WebApi.Utilities;

public class ContentTypeDetector
{
    private readonly IConfiguration _configuration;

    private static readonly Dictionary<string, ContentType> NameToType = new()
    {
        ["Picture"] = ContentType.Picture,
        ["Gif"] = ContentType.Gif,
        ["Video"] = ContentType.Video,
        ["Audio"] = ContentType.Audio
    };

    public ContentTypeDetector(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ContentType GetContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName.Trim('"')).Replace(".", "");
        var extensions = _configuration.GetSection("Extensions").GetChildren()
            .ToDictionary(x => x.Key, x => x.Get<string[]>());
        foreach (var groupExt in extensions.Keys)
        {
            if (extensions[groupExt].Contains(ext))
                return NameToType[groupExt];
        }

        return ContentType.File;
    }
}