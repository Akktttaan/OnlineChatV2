using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineChatV2.Domain;
using OnlineChatV2.WebApi.Hubs.EventManagement;
using OnlineChatV2.WebApi.Hubs.EventManagement.EventsAndReceivers;
using OnlineChatV2.WebApi.Infrastructure;
using OnlineChatV2.WebApi.Models;
using OnlineChatV2.WebApi.Services.Base;
using OnlineChatV2.WebApi.Utilities;

namespace OnlineChatV2.WebApi.Hubs;


public class ChatHub : BaseChatHub
{
    private readonly EventBus _eventBus;
    
    /// <summary>
    /// long - chatId, string - имя группы
    /// </summary>
    private readonly ConcurrentDictionary<long, string> _cachedChatIds;
    
    /// <summary>
    /// string - ConnectionId, long - userId
    /// </summary>
    private readonly ConcurrentDictionary<string, long> _cachedUsers;
    
    /// <summary>
    /// string - имя группы, List<string> - список ConnectionId
    /// </summary>
    private readonly ConcurrentDictionary<string, List<string>> _groupsMembers;
    private readonly IChatService _chatService;

    public ChatHub([FromServices] EventBus eventBus, [FromServices] IChatService service)
    {
        _groupsMembers = new();
        _cachedChatIds = new();
        _cachedUsers = new();
        _eventBus = eventBus;
        _chatService = service;
    }

    public override async Task OnConnectedAsync()
    {
        var user = GetUserFromContext(HttpContext);
        if (user == null) return;

        if (_cachedUsers.ContainsKey(Context.ConnectionId))
            return;

        _cachedUsers[Context.ConnectionId] = user.Id;
        
        await base.OnConnectedAsync();
    }

    public async Task EnterToChat(long chatId)
    {
        var groupId = await ValidateChatConnect(_chatService, chatId);
        if (groupId == null) return;
        if (_groupsMembers.ContainsKey(groupId) && _groupsMembers[groupId].Contains(Context.ConnectionId))
            return;
        var user = GetUserFromContext(HttpContext);
        if (user == null) return;
        if (!_groupsMembers.ContainsKey(groupId))
        {
            _groupsMembers[groupId] = new List<string> { Context.ConnectionId };
        }
        else
        {
            _groupsMembers[groupId].Add(Context.ConnectionId);
        }

        if (!_cachedChatIds.ContainsKey(chatId))
        {
            _cachedChatIds[chatId] = groupId;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, _cachedChatIds[chatId]);

        await Clients.Caller.SendAsync("SetChatHistory", await _chatService.GetChat(user.Id, chatId));
    }

    public async Task Send(MessageDto message)
    {
        var groupId = await ValidateChatConnect(_chatService, message.ChatId);
        if (groupId == null) return;
        var user = GetUserFromContext(HttpContext);

        var result = await _chatService.SaveMessage(user, message);

        await Clients.OthersInGroup(groupId).SendAsync("ReceiveMessage",
            _chatService.GetChatType(message.ChatId) == ChatType.Group ? message.ChatId : user.Id, result);
        await Clients.Caller.SendAsync("MessageDelivered", result.MessageDate);
        //_eventBus.Invoke(new MessageSend() {From = user.Id, To = chatId, Message = "Test"});
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
        await Clients.Caller.SendAsync("ChatCreated", result);
    }

    public async Task GetUserChats(long userId)
    {
        var user = GetUserFromContext(HttpContext);
        if (user == null) return;
        if (user.Id != userId) return;
        await Clients.Caller.SendAsync("SetChats", await _chatService.GetUserChats(userId));
    }
    
    public async Task UserWriteEvent()
    {
        _eventBus.Invoke(new UserWrite());
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_cachedUsers.ContainsKey(Context.ConnectionId))
        {
            _cachedUsers.TryRemove(Context.ConnectionId, out var id);
            foreach (var members in _groupsMembers.Values)
            {
                members.Remove(Context.ConnectionId);
            }
            // todo event user offline to bus
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    
    private async Task<string?> ValidateChatConnect(IChatService service, long chatId)
    {
        var user = GetUserFromContext(HttpContext);
        if (user == null) return null;
        var chatType = service.GetChatType(chatId);
        if (chatType == ChatType.Group && !await service.IsUserInChat(user.Id, chatId)) return null;

        switch (chatType)
        {
            case ChatType.Personal:
                var xorId = user.Id ^ chatId;
                if (_cachedChatIds.TryGetValue(xorId, out var connect))
                    return connect;;
                _cachedChatIds[xorId] = CryptoUtilities.GetMd5String(user.Id, chatId);
                return _cachedChatIds[xorId];
            case ChatType.Group:
                if (_cachedChatIds.TryGetValue(chatId, out var chatConnect))
                    return chatConnect;
                _cachedChatIds[chatId] = chatId.ToString();
                return _cachedChatIds[chatId];
            default:
                return null;
        }
    }
    
}