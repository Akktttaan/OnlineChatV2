using Microsoft.AspNetCore.SignalR;
using OnlineChatV2.Domain.Enums;
using OnlineChatV2.WebApi.Hubs.EventManagement;
using OnlineChatV2.WebApi.Hubs.EventManagement.EventsAndReceivers;
using OnlineChatV2.WebApi.Models;
using OnlineChatV2.WebApi.Services.Base;
using OnlineChatV2.WebApi.Utilities;

namespace OnlineChatV2.WebApi.Hubs;


public class ChatHub : BaseChatHub
{
    private readonly EventBus _eventBus;
    
    private readonly IChatService _chatService;
    private readonly IChatHubStore _store;
    private readonly IUserService _userService;

    public ChatHub(EventBus eventBus, IChatService service, IChatHubStore store, IUserService userService)
    {
        _eventBus = eventBus;
        _chatService = service;
        _store = store;
        _userService = userService;
    }

    // todo выяснить почему не срабатывает это дерьмо
    public override async Task OnConnectedAsync()
    {
        var user = GetUserFromContext(HttpContext);

        if (_store.UserInCache(Context.ConnectionId))
            return;

        _store.AddUser(Context.ConnectionId, user.Id);
        await Clients.All.SendAsync("UserOnline", new { Id = user.Id });
        
        await base.OnConnectedAsync();
    }
    
    public async Task Connect()
    {
        var user = GetUserFromContext(HttpContext);

        if (_store.UserInCache(Context.ConnectionId))
            return;

        _store.AddUser(Context.ConnectionId, user.Id);
        await Clients.All.SendAsync("UserOnline", user.Id);
        
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

        await Clients.Caller.SendAsync("SetChatHistory", await _chatService.GetChat(user.Id, chatId));
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
        await Clients.OthersInGroup(groupId).SendAsync("ReceiveMessage",
            fromId, result);
        await Clients.Caller.SendAsync("MessageDelivered", result.MessageDate);

        var clientsForNotify = new List<string>();
        if (_chatService.GetChatType(message.ChatId) == ChatType.Personal)
        {
            var res = _store.GetUserConnectionId(message.ChatId);
            if (res is null)
                return;
            clientsForNotify.Add(res);
        }
        else
        {
            clientsForNotify.AddRange(_store.GetGroupMembers(groupId).Where(x => x != Context.ConnectionId));
        }

        foreach (var clientId in clientsForNotify)
            await Clients.Client(clientId).SendAsync("PushNotify", new PushNotifyModel
            {
                ChatId = fromId,
                MessageText = result.MessageText,
                MessageDate = result.MessageDate,
                Sender = result.Sender,
                ChatName = result.ChatName
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
                Sender = infoResult.Sender
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