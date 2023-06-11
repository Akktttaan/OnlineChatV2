using Microsoft.EntityFrameworkCore;
using OnlineChatV2.Dal;
using OnlineChatV2.Domain;
using OnlineChatV2.Domain.Enums;
using OnlineChatV2.WebApi.Models;
using OnlineChatV2.WebApi.Models.ActionContexts;
using OnlineChatV2.WebApi.Services.Base;
using OnlineChatV2.WebApi.Utilities;

namespace OnlineChatV2.WebApi.Services.Implementation;

public class ChatService : IChatService
{
    private readonly QueryDbContext _queryDb;
    private readonly CommandDbContext _commandDb;
    private readonly IFileService _fileService;
    private readonly IWebHostEnvironment _env;
    private readonly ContentTypeDetector _detector;

    public ChatService(QueryDbContext queryDb, CommandDbContext commandDb, IFileService fileService,
        IWebHostEnvironment env, ContentTypeDetector detector)
    {
        _queryDb = queryDb;
        _commandDb = commandDb;
        _fileService = fileService;
        _env = env;
        _detector = detector;
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
            OwnerId = model.CreatedById,
            Description = model.Description
        };
        await _commandDb.AddAsync(chat);
        await _commandDb.SaveChangesAsync();
        string? url = null;
        if (model.Avatar != null)
        {
            url = await _fileService.UploadAvatar(chat.Id, model.Avatar, _env.WebRootPath, AvatarType.Chat);
            chat.CurrentAvatar = url;
            var chatAvatar = new ChatAvatar()
            {
                ChatId = chat.Id,
                AvatarUrl = url
            };
            await _commandDb.ChatAvatars.AddAsync(chatAvatar);
            _commandDb.Chats.Update(chat);
            await _commandDb.SaveChangesAsync();
        }
        
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
            WhoAdded = chatUsers.Select(x => x.UserId),
            Description = model.Description,
            AvatarUrl = url
        };
    }

    public async Task<ChatModel[]> GetUserChats(long userId)
    {
        var userChats = _queryDb.ChatUsers.Where(x => x.UserId == userId)
            .Include(x => x.Chat)
            .Select(x => new { Id = x.ChatId, Name = x.Chat.Name, AvatarUrl = x.Chat.CurrentAvatar });

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
            join fullMessage in _queryDb.Messages.Include(x => x.FromUser).Include(x => x.MessageContent) 
                on lastMessage.MessageId equals fullMessage.Id into fullMessageGroup
            from fullMessage in fullMessageGroup.DefaultIfEmpty()
            select new ChatModel()
            {
                Id = chat.Id,
                Name = chat.Name,
                LastMessageDate = fullMessage.MessageDate,
                LastMessageFromSender = fullMessage.FromUserId == userId,
                LastMessageSenderName = fullMessage.FromUser.Username,
                LastMessageText = fullMessage.MessageText,
                AvatarUrl = chat.AvatarUrl,
                LastMessageWithContent = fullMessage.MessageContent == null,
                LastMessageContentType = fullMessage.MessageContent == null ? null : (ContentType)fullMessage.MessageContent.ContentTypeId
            });
        
        var lastMessageFromPm = from m in fr.Union(to)
            group m by m.ChatId into g
            select new { PrivateChatId = g.Key, MessageId = g.Max(m => m.Id) };

        var pmsWithLastMessages = from chat in lastMessageFromPm
            join lastmessage in _queryDb.Messages.Include(x => x.MessageContent) 
                on chat.MessageId equals lastmessage.Id
            join user in _queryDb.Users on chat.PrivateChatId equals user.Id
            select new ChatModel()
            {
                Id = chat.PrivateChatId,
                Name = user.Username,
                LastMessageDate = lastmessage.MessageDate,
                LastMessageFromSender = lastmessage.FromUserId == userId,
                LastMessageSenderName = null,
                LastMessageText = lastmessage.MessageText,
                AvatarUrl = user.CurrentAvatar,
                LastMessageWithContent = lastmessage.MessageContent == null,
                LastMessageContentType = lastmessage.MessageContent == null ? null : (ContentType)lastmessage.MessageContent.ContentTypeId
            };

        return await chatsWithLastMessage.Union(pmsWithLastMessages)
            .OrderByDescending(x => x.LastMessageDate.HasValue)
            .ThenByDescending(x => x.LastMessageDate)
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
            .Include(x => x.ToUser)
            .Where(x => x.ToUserId == chatId && x.FromUserId == userId ||
                        x.FromUserId == chatId && x.ToUserId == userId);
        return await MapHistory(data);
    }

    private async Task<ChatHistoryModel[]> MapHistory(IQueryable<Message> filteredHistory)
    {
        return await filteredHistory.Include(x => x.FromUser)
            .Include(x => x.MessageContent)
            .Select(x => new ChatHistoryModel
            {
                MessageId = x.Id,
                MessageText = x.MessageText,
                MessageDate = x.MessageDate,
                MessageType = x.MessageType,
                ContentPath = x.MessageContent == null ? string.Empty : x.MessageContent.ContentPath,
                ContentType = x.MessageContent == null ? null : x.MessageContent.ContentType,
                Sender = new SenderModel
                {
                    UserId = x.FromUserId,
                    AvatarUrl = x.FromUser.CurrentAvatar,
                    Username = x.FromUser.Username,
                    NicknameColor = x.FromUser.NicknameColor
                }
            }).OrderBy(x => x.MessageDate).ToArrayAsync();
    }

    public async Task<ChatHistoryModel> SaveMessage(User user, MessageDto message, MessageType type = MessageType.Common)
    {
        var entry = new Message()
        {
            FromUserId = user.Id,
            MessageDate = DateTime.Now.ToUniversalTime(),
            MessageText = message.MessageText,
            MessageType = type
        };
        string? chatName;
        if (GetChatType(message.ChatId) == ChatType.Personal)
        {
            entry.ToUserId = message.ChatId;
            chatName = (await _commandDb.Users.FindAsync(message.ChatId))?.Username;
        }
        else
        {
            entry.ChatId = message.ChatId;
            chatName = (await _commandDb.Chats.FindAsync(message.ChatId))?.Name;
        }
        await _commandDb.AddAsync(entry);
        await _commandDb.SaveChangesAsync();
        var result = new ChatHistoryModel()
        {
            MessageText = entry.MessageText,
            MessageDate = entry.MessageDate,
            MessageId = entry.Id,
            ChatName = chatName,
            MessageType = entry.MessageType,
            Sender = new SenderModel()
            {
                AvatarUrl = user.CurrentAvatar,
                UserId = user.Id,
                Username = user.Username,
                NicknameColor = user.NicknameColor
            }
        };
        if (message.Content != null)
        {
            var contentType = _detector.GetContentType(message.Content.Name);
            var filePath = await _fileService.SaveFile(message.Content, _env.WebRootPath);
            var content = new MessageContent()
            {
                MessageId = entry.Id,
                ContentType = contentType,
                ContentPath = filePath
            };
            await _commandDb.MessageContent.AddAsync(content);
            await _commandDb.SaveChangesAsync();
            result.MessageType = MessageType.WithContent;
            result.ContentType = contentType;
            result.ContentPath = filePath;
        }

        return result;
    }

    public async Task<ChatInfo> GetChatInfo(long chatId)
    {
        var chat = await _queryDb.Chats.FirstOrError<Chat, ArgumentException>(x => x.Id == chatId, $"Чат с id {chatId} не найден!");
        var chatUsers = await _queryDb.ChatUsers
            .Where(x => x.ChatId == chatId)
            .Include(x => x.User)
            .ThenInclude(x => x.UserAvatars)
            .Select(x =>
            new ChatMember()
            {
                UserId = x.UserId,
                UserName = x.User.Username,
                AvatarUrl = x.User.CurrentAvatar
            }).ToArrayAsync();
        return new ChatInfo()
        {
            ChatId = chat.Id,
            ChatName = chat.Name,
            Members = chatUsers,
            OwnerId = chat.OwnerId,
            ChatDescription = chat.Description,
            AvatarUrl = chat.CurrentAvatar
        };
    }

    public async Task MoveUserInChat(long chatId, long userId, ChatAction action)
    {
        switch (action)
        {
            case ChatAction.AddUser:
                await AddUserToChat(chatId, userId);
                break;
            case ChatAction.RemoveUser:
            case ChatAction.UserLeave:    
                await RemoveUserFromChat(chatId, userId);
                break;
            default:
                throw new Exception($"Не найден обработчик для {action.ToString()}");
        }
    }

    public async Task<bool> IsHavePermission(long userId, long chatId)
    {
        var chat = await _queryDb.Chats.FindAsync(chatId);
        return chat != null && chat.OwnerId == userId;
    }

    private async Task AddUserToChat(long chatId, long userId)
    {
        var chatUser = await _commandDb.ChatUsers.FirstOrDefaultAsync(x => x.UserId == userId && x.ChatId == chatId);
        var user = await _commandDb.Users.FindAsync(userId);
        if (chatUser != null || user == null)
            return;
        var entry = new ChatUser()
        {
            ChatId = chatId,
            UserId = userId
        };
        await _commandDb.AddAsync(entry);
        await _commandDb.SaveChangesAsync();
    }
    
    private async Task RemoveUserFromChat(long chatId, long userId)
    {
        var chatUser = await _commandDb.ChatUsers.FirstOrDefaultAsync(x => x.UserId == userId && x.ChatId == chatId);
        if (chatUser == null)
            return;
        _commandDb.Remove(chatUser);
        await _commandDb.SaveChangesAsync();
    }

    public async Task<MessageDto> GenerateChatActionMessage(BaseActionContext context, ChatAction action)
    {
        string result;
        switch (action)
        {
            case ChatAction.RemoveUser:
            case ChatAction.AddUser:
                if (context is not UserMoveActionContext moveContext)
                    throw new Exception("Некорректный контекст");
                var target = await _queryDb.Users.FindAsync(moveContext.TargetId) ??
                             throw new Exception("Произошла ошибка - пользователь не найден");
                result = $"{moveContext.Invoker.Username} {action.GetEnumInfo()} {target.Username}";
                break;
            case ChatAction.UserLeave:
                if (context is not UserMoveActionContext moveActionContext)
                    throw new Exception("Некорректный контекст");
                var leaveTarget = await _queryDb.Users.FindAsync(moveActionContext.TargetId) ??
                             throw new Exception("Произошла ошибка - пользователь не найден");
                result = $"{leaveTarget.Username} {action.GetEnumInfo()}";
                break;
            case ChatAction.ChangeGroup:
                result = $"{context.Invoker.Username} {action.GetEnumInfo()}";
                break;
            default:
                throw new NotImplementedException($"Не реализован обработчик для {action.GetEnumInfo()}");
        }

        return new MessageDto()
        {
            ChatId = context.ChatId,
            MessageText = result
        };
    }
    
    public async Task UploadAvatar(long chatId, IFormFile photo)
    {
        var chat = await _queryDb.Chats.FirstOrError<Chat, ArgumentException>(x => x.Id == chatId,
            "Пользователь не найден");
        var avatarPath = await _fileService.UploadAvatar(chatId, photo, _env.WebRootPath, AvatarType.User);
        var avatar = new ChatAvatar()
        {
            AvatarUrl = avatarPath,
            ChatId = chatId
        };
        await _commandDb.ChatAvatars.AddAsync(avatar);
        await _commandDb.SaveChangesAsync();
        chat.CurrentAvatar = avatar.AvatarUrl;
        _commandDb.Chats.Update(chat);
        await _commandDb.SaveChangesAsync();
    }
    
    public async Task UploadAvatar(long chatId, FileModel photo)
    {
        var chat = await _queryDb.Chats.FirstOrError<Chat, ArgumentException>(x => x.Id == chatId,
            "Пользователь не найден");
        var avatarPath = await _fileService.UploadAvatar(chatId, photo, _env.WebRootPath, AvatarType.User);
        var avatar = new ChatAvatar()
        {
            AvatarUrl = avatarPath,
            ChatId = chatId
        };
        await _commandDb.ChatAvatars.AddAsync(avatar);
        await _commandDb.SaveChangesAsync();
        chat.CurrentAvatar = avatar.AvatarUrl;
        _commandDb.Chats.Update(chat);
        await _commandDb.SaveChangesAsync();
    }
    
    public async Task UpdateAbout(long chatId, string about)
    {
        var chat = await _queryDb.Chats.FirstOrError<Chat, ArgumentException>(x => x.Id == chatId,
            "Чат не найден");
        chat.Description = about;
        _commandDb.Chats.Update(chat);
        await _commandDb.SaveChangesAsync();
    }
    
    public async Task ChangeName(long chatId, string newName)
    {
        var chat = await _queryDb.Chats.FirstOrError<Chat, ArgumentException>(x => x.Id == chatId,
            "Чат не найден");
        chat.Name = newName;
        _commandDb.Chats.Update(chat);
        await _commandDb.SaveChangesAsync();
    }
    
    public async Task<string?> GetChatAvatar(long chatId)
    {
        return (await _queryDb.Chats.FirstOrError<Chat, ArgumentException>(x => x.Id == chatId, "Чат не найден"))
            .CurrentAvatar;
    }
}