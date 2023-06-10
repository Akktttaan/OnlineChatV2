using OnlineChatV2.WebApi.Infrastructure;

namespace OnlineChatV2.WebApi.Models;

public enum AvatarType
{
    [EnumInfo("userAvatars")]
    User,
    [EnumInfo("chatAvatars")]
    Chat
}