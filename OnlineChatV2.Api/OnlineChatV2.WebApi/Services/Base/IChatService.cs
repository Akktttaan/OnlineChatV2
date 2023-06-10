using OnlineChatV2.Domain;
using OnlineChatV2.Domain.Enums;
using OnlineChatV2.WebApi.Models;
using OnlineChatV2.WebApi.Models.ActionContexts;

namespace OnlineChatV2.WebApi.Services.Base;

public interface IChatService
{
    ChatType GetChatType(long chatId);
    
    Task<bool> IsUserInChat(long userId, long chatId);
    
    Task<bool> IsChatExist(long chatId);
    Task<CreateChatResult> CreateChat(CreateChatModel model);

    Task<ChatModel[]> GetUserChats(long userId);

    Task<ChatHistoryModel[]> GetChat(long userId, long chatId);

    Task<ChatHistoryModel> SaveMessage(User user, MessageDto message, MessageType type = MessageType.Common);

    Task<ChatInfo> GetChatInfo(long chatId);
    Task MoveUserInChat(long chatId, long userId, ChatAction action);
    Task<MessageDto> GenerateChatActionMessage(BaseActionContext context, ChatAction action);
    Task<bool> IsHavePermission(long userId, long chatId);
    Task UploadAvatar(long chatId, IFormFile photo, IWebHostEnvironment env);
    Task ChangeName(long chatId, string newName);
    Task UpdateAbout(long chatId, string about);
}