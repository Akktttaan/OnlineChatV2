using OnlineChatV2.WebApi.Models;

namespace OnlineChatV2.WebApi.Services.Base;

public interface IChatService
{
    ChatType GetChatType(long chatId);
    
    Task<bool> IsUserInChat(long userId);
    
    Task<bool> IsChatExist(long chatId);
}