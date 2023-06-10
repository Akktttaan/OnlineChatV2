using OnlineChatV2.WebApi.Infrastructure;

namespace OnlineChatV2.WebApi.Models;

public enum ChatAction
{
    [EnumInfo("добавил(а)")]
    AddUser,
    [EnumInfo("удалил(а)")]
    RemoveUser,
    [EnumInfo("покинул(а) группу")]
    UserLeave,
    [EnumInfo("изменил(а) название группы на")]
    ChangeGroup,
    [EnumInfo("изменил(а) аватар группы")]
    ChangeAvatar,
}