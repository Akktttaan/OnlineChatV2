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

    public async Task<bool> IsUserInChat(long userId, long chatId)
    {
        return await _queryDb.ChatUsers
                    .Where(x => x.UserId == userId && x.ChatId == chatId)
                    .FirstOrDefaultAsync() is not null;
    }

    public async Task<bool> IsChatExist(long chatId)
    {
        var user = await _queryDb.Users.FindAsync(chatId) is not null;
        var chat = await _queryDb.Chats.FindAsync(chatId) is not null;
        return user || chat;
    }

    public async Task<CreateChatResult> CreateChat(CreateChatModel model)
    {
        if (await _commandDb.Users.FindAsync(model.CreatedById) == null)
            throw new ArgumentException("Not valid owner id");
        
        var chat = new Chat
        {
            Name = model.ChatName,
            OwnerId = model.CreatedById
        };
        await _commandDb.AddAsync(chat);
        await _commandDb.SaveChangesAsync();
        
        var chatUsers = new LinkedList<ChatUser>();
        foreach (var userId in model.ChatUserIds)
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
        var userChats = _queryDb.ChatUsers.Where(x => x.UserId == userId)
            .Include(x => x.Chat)
            .Select(x => new { Id = x.ChatId, Name = x.Chat.Name });

        var fr = from m1 in _queryDb.Messages
            where m1.FromUserId == userId && m1.ChatId == null
            select new { Id = m1.Id, ChatId = m1.ToUserId.Value };
        

        var to =
            from m2 in _queryDb.Messages
            where m2.ToUserId == userId && m2.ChatId == null
            select new { Id = m2.Id, ChatId = m2.FromUserId };

        var lastMessageFromChats = from chat in userChats
           join message in _queryDb.Messages on chat.Id equals message.ChatId
           group message by message.ChatId into g
           select new { ChatId = g.Key, MessageId = g.Max(x => x.Id) };

        var chatsWithLastMessage = (from chat in userChats
            join lastMessage in lastMessageFromChats on chat.Id equals lastMessage.ChatId into lastMessageGroup
            from lastMessage in lastMessageGroup.DefaultIfEmpty()
            join fullMessage in _queryDb.Messages.Include(x => x.FromUser) on lastMessage.MessageId equals fullMessage.Id into fullMessageGroup
            from fullMessage in fullMessageGroup.DefaultIfEmpty()
            select new ChatModel()
            {
                Id = chat.Id,
                Name = chat.Name,
                LastMessageDate = fullMessage.MessageDate,
                LastMessageFromSender = fullMessage.FromUserId == userId,
                LastMessageSenderName = fullMessage.FromUser.Username,
                LastMessageText = fullMessage.MessageText
            });
        
        var lastMessageFromPm = from m in fr.Union(to)
            group m by m.ChatId into g
            select new { PrivateChatId = g.Key, MessageId = g.Max(m => m.Id) };

        var pmsWithLastMessages = from chat in lastMessageFromPm
            join lastmessage in _queryDb.Messages on chat.MessageId equals lastmessage.Id
            join user in _queryDb.Users on chat.PrivateChatId equals user.Id
            select new ChatModel()
            {
                Id = chat.PrivateChatId,
                Name = user.Username,
                LastMessageDate = lastmessage.MessageDate,
                LastMessageFromSender = lastmessage.FromUserId == userId,
                LastMessageSenderName = null,
                LastMessageText = lastmessage.MessageText
            };

        return await chatsWithLastMessage.Union(pmsWithLastMessages)
            .OrderByDescending(x => x.LastMessageDate)
            .Where(x => x.Id != null)
            .ToArrayAsync();
    }

    public async Task<ChatHistoryModel[]> GetChat(long userId, long chatId)
    {
        var chatType = GetChatType(chatId);
        return chatType switch
        {
            ChatType.Group => await GetGroupChatHistory(chatId),
            ChatType.Personal => await GetPrivateChatHistory(userId, chatId),
            _ => Array.Empty<ChatHistoryModel>()
        };
    }

    private async Task<ChatHistoryModel[]> GetGroupChatHistory(long chatId)
    {
        var data = _queryDb.Messages.Where(x => x.ChatId == chatId);
        return await MapHistory(data);
    }
    
    private async Task<ChatHistoryModel[]> GetPrivateChatHistory(long userId, long chatId)
    {
        var data = _queryDb.Messages
            .Where(x => x.ToUserId == chatId && x.FromUserId == userId ||
                        x.FromUserId == chatId && x.ToUserId == userId);
        return await MapHistory(data);
    }

    private async Task<ChatHistoryModel[]> MapHistory(IQueryable<Message> filteredHistory)
    {
        return await filteredHistory.Include(x => x.FromUser)
            .Include(x => x.ToUser)
            .Select(x => new ChatHistoryModel
            {
                MessageId = x.Id,
                MessageText = x.MessageText,
                MessageDate = x.MessageDate,
                Sender = new SenderModel
                {
                    UserId = x.FromUserId,
                    AvatarUrl = string.Empty,
                    Username = x.FromUser.Username
                }
            }).OrderBy(x => x.MessageDate).ToArrayAsync();
    }

    public async Task<ChatHistoryModel> SaveMessage(User user, MessageDto message)
    {
        var entry = new Message()
        {
            FromUserId = user.Id,
            MessageDate = DateTime.Now,
            MessageText = message.MessageText
        };
        if (GetChatType(message.ChatId) == ChatType.Personal)
        {
            entry.ToUserId = message.ChatId;
        }
        else
        {
            entry.ChatId = message.ChatId;
        }

        await _commandDb.AddAsync(entry);
        await _commandDb.SaveChangesAsync();
        return new ChatHistoryModel()
        {
            MessageText = entry.MessageText,
            MessageDate = entry.MessageDate,
            MessageId = entry.Id,
            Sender = new SenderModel()
            {
                AvatarUrl = "",
                UserId = user.Id,
                Username = user.Username
            }
        };
    }
    
 }