using System.Security.Authentication;
using Microsoft.AspNetCore.SignalR;
using OnlineChatV2.Domain;
using OnlineChatV2.Domain.Enums;
using OnlineChatV2.WebApi.Hubs.EventManagement;
using OnlineChatV2.WebApi.Hubs.EventManagement.EventsAndReceivers;
using OnlineChatV2.WebApi.Models;
using OnlineChatV2.WebApi.Models.ActionContexts;
using OnlineChatV2.WebApi.Services.Base;
using OnlineChatV2.WebApi.Utilities;

namespace OnlineChatV2.WebApi.Hubs;


public class ChatHub : BaseChatHub
{
    private readonly EventBus _eventBus;
    
    private readonly IChatService _chatService;
    private readonly IChatHubStore _store;
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _environment;

    public ChatHub(EventBus eventBus, IChatService service, IChatHubStore store, IUserService userService, IWebHostEnvironment environment)
    {
        _eventBus = eventBus;
        _chatService = service;
        _store = store;
        _userService = userService;
        _environment = environment;
    }

    // todo выяснить почему не срабатывает это дерьмо
    public override async Task OnConnectedAsync()
    {
        var user = GetUserFromContext(HttpContext);

        if (_store.UserInCache(Context.ConnectionId))
            return;

        _store.AddUser(Context.ConnectionId, user.Id);
        await Clients.Others.SendAsync("UserOnline", user.Id);
        await Clients.Caller.SendAsync("SetUsersOnline", _store.GetOnlineUsers());
        
        await base.OnConnectedAsync();
    }
    
    public async Task Connect()
    {
        var user = GetUserFromContext(HttpContext);

        if (_store.UserInCache(Context.ConnectionId))
            return;

        _store.AddUser(Context.ConnectionId, user.Id);
        await Clients.Others.SendAsync("UserOnline", user.Id);
        await Clients.Caller.SendAsync("SetUsersOnline", _store.GetOnlineUsers());
        
        await base.OnConnectedAsync();;
    }

    public async Task EnterToChat(long chatId)
    {
        var groupId = await ValidateChatConnect(_chatService, chatId);
        if (groupId == null) return;
        if (_store.GroupExist(groupId) && _store.UserInGroup(groupId, Context.ConnectionId))
            return;
        var user = GetUserFromContext(HttpContext);
        if (!_store.GroupExist(groupId))
        {
            _store.AddGroup(chatId, groupId);
        }
        _store.AddUserToGroup(Context.ConnectionId, groupId);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        var history = await _chatService.GetChat(user.Id, chatId);
        await Clients.Caller.SendAsync("SetChatHistory", history);
        if(_chatService.GetChatType(chatId) == ChatType.Group)
            await Clients.Caller.SendAsync("SetChatInfo", await _chatService.GetChatInfo(chatId));
        if (_chatService.GetChatType(chatId) == ChatType.Personal)
            await Clients.Caller.SendAsync("SetUserInfo", await _userService.GetUserInfo(chatId));
    }

    public async Task Send(MessageDto message)
    {
        var groupId = await ValidateChatConnect(_chatService, message.ChatId);
        if (groupId == null) return;
        var user = GetUserFromContext(HttpContext);

        var result = await _chatService.SaveMessage(user, message);

        var fromId = _chatService.GetChatType(message.ChatId) == ChatType.Group ? message.ChatId : user.Id;
        await InvokeSendOnClients(new InvokeSendContext
        {
            FromId = fromId,
            GroupId = groupId,
            Message = result,
            ToChatId = message.ChatId, 
            AvatarUrl = _chatService.GetChatType(message.ChatId) == ChatType.Group ? await _chatService.GetChatAvatar(message.ChatId) : user.CurrentAvatar,
            NotifyInvoker = false
        });
    }

    public async Task AddUser(long chatId, IEnumerable<long> userIds)
    {
        var user = GetUserFromContext(HttpContext);
        foreach (var userId in userIds)
        {
            await _chatService.MoveUserInChat(chatId, userId, ChatAction.AddUser);
            if (_store.TryGetGroupId(chatId, out var groupId) && groupId != null)
            {
                var connId = _store.GetUserConnectionId(userId);
                if (connId != null)
                {
                    _store.AddUserToGroup(connId, groupId);
                    await Groups.AddToGroupAsync(connId, groupId);
                }
                await InvokeActionOnClients(new UserMoveActionContext
                {
                    ChatId = chatId,
                    Invoker = user,
                    TargetId = userId
                }, ChatAction.AddUser);
            }

        }
    }

    public async Task RemoveUser(long chatId, long userId) 
    {
        var user = GetUserFromContext(HttpContext);
        if (!await _chatService.IsHavePermission(user.Id, chatId))
            throw new AuthenticationException("Нет прав для удаления");
        if (_store.TryGetGroupId(chatId, out var groupId) && groupId != null)
        {
            await InvokeActionOnClients(new UserMoveActionContext
            {
                ChatId = chatId,
                Invoker = user,
                TargetId = userId
            }, ChatAction.RemoveUser);
            _store.RemoveUserFromGroup(userId, groupId);
            var connId = _store.GetUserConnectionId(userId);
            if (connId != null)
                await Clients.Client(connId).SendAsync("KickedFromChat", chatId);
        }
        await _chatService.MoveUserInChat(chatId, user.Id, ChatAction.RemoveUser);
    }

    public async Task LeaveChat(long chatId)
    {
        var user = GetUserFromContext(HttpContext);
        if (_store.TryGetGroupId(chatId, out var groupId) && groupId != null)
        {
            await InvokeActionOnClients(new UserMoveActionContext()
            {
                ChatId = chatId,
                Invoker = user,
                TargetId = user.Id
            }, ChatAction.UserLeave, false);
            _store.RemoveUserFromGroup(user.Id, groupId);
        }
        await _chatService.MoveUserInChat(chatId, user.Id, ChatAction.UserLeave);
    }

    public async Task ChangeChatName(long chatId, string newName)
    {
        var user = GetUserFromContext(HttpContext);
        if (!await _chatService.IsHavePermission(user.Id, chatId))
            throw new AuthenticationException("Нет прав");
        await _chatService.ChangeName(chatId, newName);
        if (_store.TryGetGroupId(chatId, out var groupId) && groupId != null)
        {
            await InvokeActionOnClients(new ChangeNameActionContext()
            {
                ChatId = chatId,
                Invoker = user,
                NewName = newName,
            }, ChatAction.ChangeGroup);
            await Clients.Group(groupId).SendAsync("SetChatName", chatId, newName);
        }
    }

    public async Task ChangeChatAvatar(long chatId, FileModel photo)
    {
        var user = GetUserFromContext(HttpContext);
        if (!await _chatService.IsHavePermission(user.Id, chatId))
            throw new AuthenticationException("Нет прав");
        if (_store.TryGetGroupId(chatId, out var groupId) && groupId != null)
        {
            await InvokeActionOnClients(new BaseActionContext()
            {
                ChatId = chatId,
                Invoker = user,
            }, ChatAction.ChangeAvatar);
        }

        await _chatService.UploadAvatar(chatId, photo);
    }
    
    public async Task UpdateAbout(long chatId, string about)
    {
        var user = GetUserFromContext(HttpContext);
        if (!await _chatService.IsHavePermission(user.Id, chatId))
            throw new AuthenticationException("Нет прав");
        if (_store.TryGetGroupId(chatId, out var groupId) && groupId != null)
        {
            await Clients.Group(groupId).SendAsync("AboutChanged", about);
        }
        await _chatService.UpdateAbout(chatId, about);
    }

    private async Task InvokeActionOnClients(BaseActionContext context, ChatAction action, bool notifyInvoker = true)
    {
        var groupId = await ValidateChatConnect(_chatService, context.ChatId);
        if (groupId == null) return;
        if (_chatService.GetChatType(context.ChatId) == ChatType.Personal)
            return;
        var dto = await _chatService.GenerateChatActionMessage(context, action);
        var result = await _chatService.SaveMessage(context.Invoker, dto, MessageType.System);
        await InvokeSendOnClients(new InvokeSendContext()
        {
            FromId = context.ChatId,
            GroupId = groupId,
            Message = result,
            ToChatId = context.ChatId,
            AvatarUrl = await _chatService.GetChatAvatar(context.ChatId),
            NotifyInvoker = notifyInvoker // оповещаем в т.ч того кто вызвал
        });
    }
    
    private async Task InvokeSendOnClients(InvokeSendContext context)
    {
        if (context.NotifyInvoker)
        {
            await Clients.Group(context.GroupId).SendAsync("ReceiveMessage",
                context.FromId, context.Message);
        }
        else
            await Clients.OthersInGroup(context.GroupId).SendAsync("ReceiveMessage",
            context.FromId, context.Message);
        await Clients.Caller.SendAsync("MessageDelivered", context.Message.MessageDate);
        await Clients.Others.SendAsync("UserOnline", context.Message.Sender.UserId);
        var clientsForNotify = new List<string>();
        if (_chatService.GetChatType(context.ToChatId) == ChatType.Personal)
        {
            var res = _store.GetUserConnectionId(context.ToChatId);
            if (res is null)
                return;
            clientsForNotify.Add(res);
        }
        else
        {
            clientsForNotify.AddRange(_store.GetGroupMembers(context.GroupId)
                .Where(x => x != Context.ConnectionId || context.NotifyInvoker));
        }

        foreach (var clientId in clientsForNotify)
            await Clients.Client(clientId).SendAsync("PushNotify", new PushNotifyModel
            {
                ChatId = context.FromId,
                MessageText = context.Message.MessageText,
                MessageDate = context.Message.MessageDate,
                Sender = context.Message.Sender,
                ChatName = context.Message.ChatName,
                AvatarUrl = context.AvatarUrl
            });
    }

    public async Task CreateChat(CreateChatModel model)
    {
        var result = await _chatService.CreateChat(model);
        _eventBus.Invoke(new ChatCreated()
        {
            ChatId = result.Id,
            ChatName = model.ChatName,
            WhoAdded = result.WhoAdded
        });
        var user = GetUserFromContext(HttpContext);
        await Clients.Caller.SendAsync("ChatCreated", result);
        var infoMsg = new MessageDto()
        {
            ChatId = result.Id,
            MessageText = $"{user.Username} создал чат {model.ChatName}"
        };
        var infoResult = await _chatService.SaveMessage(user, infoMsg, MessageType.System);
        foreach (var userId in result.WhoAdded)
        {
            var connectionId = _store.GetUserConnectionId(userId);
            if(connectionId == null)
                continue;
            await Clients.Client(connectionId).SendAsync("PushNotify", new PushNotifyModel()
            {
                ChatId = result.Id,
                ChatName = model.ChatName,
                MessageDate = infoResult.MessageDate,
                MessageText = infoResult.MessageText,
                Sender = infoResult.Sender,
                AvatarUrl = result.AvatarUrl
            });
        }
    }

    public async Task GetUserChats(long userId)
    {
        var user = GetUserFromContext(HttpContext);
        if (user.Id != userId) return;
        var result = await _chatService.GetUserChats(userId);
        await Clients.Caller.SendAsync("SetChats", result);
    }
    
    public async Task UserWriteEvent()
    {
        _eventBus.Invoke(new UserWrite());
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await HandleUserDisconnection(Context);
        
        _store.RemoveUser(Context.ConnectionId);
        _store.RemoveUserFromGroups(Context.ConnectionId);
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task OnInactiveDisconnect()
    {
        await this.OnDisconnectedAsync(null);
    }

    private async Task HandleUserDisconnection(HubCallerContext context)
    {
        var id = _store.GetUserId(context.ConnectionId);
        if (id.HasValue)
        {
            var time = await _userService.UpdateLastSeenTime(id.Value);
            await Clients.All.SendAsync("UserOffline", id, time);
        }
    }
    
    private async Task<string?> ValidateChatConnect(IChatService service, long chatId)
    {
        var user = GetUserFromContext(HttpContext);
        var chatType = service.GetChatType(chatId);
        if (chatType == ChatType.Group && !await service.IsUserInChat(user.Id, chatId)) return null;

        switch (chatType)
        {
            case ChatType.Personal:
                var xorId = user.Id ^ chatId;
                if (_store.TryGetGroupId(xorId, out var connect))
                    return connect;
                var groupId = CryptoUtilities.GetMd5String(user.Id, chatId);
                _store.AddGroup(xorId, groupId);
                return groupId;
            case ChatType.Group:
                if (_store.TryGetGroupId(chatId, out var chatConnect))
                    return chatConnect;
                var chatGroupId = chatId.ToString();
                _store.AddGroup(chatId, chatGroupId);
                return chatGroupId;
            default:
                return null;
        }
    }

}