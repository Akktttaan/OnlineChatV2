using OnlineChatV2.Dal;
using OnlineChatV2.WebApi.Models;
using OnlineChatV2.WebApi.Services.Base;

namespace OnlineChatV2.WebApi.Services.Implementation;

public class ChatService : IChatService
{
    private readonly QueryDbContext _queryDb;
    private readonly CommandDbContext _commandDb;

    public ChatService(QueryDbContext queryDb, CommandDbContext commandDb)
    {
        _queryDb = queryDb;
        _commandDb = commandDb;
    }

    public ChatType GetChatType(long chatId)
    {
        return chatId >= 0 ? ChatType.Personal : ChatType.Group;
    }

    public async Task<bool> IsUserInChat(long userId)
    {
        return await _queryDb.ChatUsers.FindAsync(userId) is null;
    }

    public async Task<bool> IsChatExist(long chatId)
    {
        var user = await _queryDb.Users.FindAsync(chatId) is not null;
        var chat = await _queryDb.Chats.FindAsync(chatId) is not null;
        return user || chat;
    }
}