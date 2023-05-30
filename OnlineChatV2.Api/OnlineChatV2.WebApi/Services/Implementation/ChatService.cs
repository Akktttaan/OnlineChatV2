using Microsoft.EntityFrameworkCore;
using OnlineChatV2.Dal;
using OnlineChatV2.Domain;
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

    public async Task<CreateChatResult> CreateChat(CreateChatModel model)
    {
        if (await _commandDb.Users.FindAsync(model.CreateById) == null)
            throw new ArgumentException("Not valid owner id");
        
        var chat = new Chat
        {
            Name = model.ChatName,
            OwnerId = model.CreateById
        };
        await _commandDb.AddAsync(chat);
        await _commandDb.SaveChangesAsync();
        
        var chatUsers = new LinkedList<ChatUser>();
        foreach (var userId in model.ChatUsersId)
        {
            var user = await _commandDb.Users.FindAsync(userId);
            if (user == null)
                continue;
            var chatUser = new ChatUser()
            {
                ChatId = chat.Id,
                UserId = user.Id
            };
            chatUsers.AddLast(chatUser);
        }

        await _commandDb.AddRangeAsync(chatUsers);
        await _commandDb.SaveChangesAsync();
        return new CreateChatResult
        {
            Id = chat.Id,
            WhoAdded = chatUsers.Select(x => x.UserId)
        };
    }

    public async Task<ChatModel[]> GetUserChats(long userId)
    {
        var userChats = _queryDb.ChatUsers.Where(x => x.UserId == userId).Include(x => x.Chat)
            .Select(x => new { Id = x.ChatId, Name = x.Chat.Name });
        var userPrivateChats = _queryDb.Messages.Where(x => x.FromUserId == userId && x.ChatId == null).Include(x => x.ToUser)
            .DistinctBy(x => x.ToUserId).Select(x => new { x.ToUserId, x.FromUser.Username});
        var lastMessageFromChats = from chat in userChats
            join message in _queryDb.Messages on chat.Id equals message
                .ChatId
            group message by message.ChatId
            into g
            select new { ChatId = g.Key, MessageId = g.Max(x => x.Id) };
        var chatsWithLastMessage = (from chat in userChats
            join lastMessage in lastMessageFromChats on chat.Id equals lastMessage.ChatId
            join fullMessage in _queryDb.Messages.Include(x => x.FromUser) on lastMessage.MessageId equals fullMessage
                .Id
            select new ChatModel()
            {
                Id = chat.Id, Name = chat.Name, LastMessageDate = fullMessage.MessageDate,
                LastMessageFromSender = fullMessage.FromUserId == userId,
                LastMessageSenderName = fullMessage.FromUser.Username,
                LastMessageText = fullMessage.MessageText
            });
        var lastMessageFromPM = from pm in userPrivateChats
            join message in _queryDb.Messages on pm.ToUserId equals message.ToUserId
            group message by message.ToUserId
            into g
            select new { PrivateChatId = g.Key, MessageId = g.Max(x => x.Id) };
        var pmsWithLastMessages = (from chat in userPrivateChats
            join lastMessage in lastMessageFromPM on chat.ToUserId equals lastMessage.PrivateChatId
            join fullMessage in _queryDb.Messages.Include(x => x.FromUser) on lastMessage.MessageId equals fullMessage.Id
            select new ChatModel()
            {
                Id = chat.ToUserId.Value, Name = chat.Username, LastMessageDate = fullMessage.MessageDate,
                LastMessageFromSender = fullMessage.FromUserId == userId,
                LastMessageSenderName = fullMessage.FromUser.Username,
                LastMessageText = fullMessage.MessageText
            });
        return await chatsWithLastMessage.Union(pmsWithLastMessages).OrderByDescending(x => x.LastMessageDate).ToArrayAsync();
    }
 }