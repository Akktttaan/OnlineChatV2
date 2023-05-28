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
    private readonly ConcurrentDictionary<string, List<string>> _chats;
    private readonly ConcurrentDictionary<long, string> _cachedChatIds;
    private readonly ConcurrentDictionary<string, long> _cachedUsers;
    private readonly IChatService _chatService;

    public ChatHub([FromServices] EventBus eventBus, [FromServices] IChatService service)
    {
        _chats = new();
        _cachedChatIds = new();
        _cachedUsers = new();
        _eventBus = eventBus;
        _chatService = service;
    }

    [Authorize]
    public override async Task OnConnectedAsync()
    {
        var user = GetUserFromContext(HttpContext);
        if (user == null) return;

        if (_cachedUsers.ContainsKey(Context.ConnectionId))
            return;

        _cachedUsers[Context.ConnectionId] = user.Id;
        
        await base.OnConnectedAsync();
    }

    [Authorize]
    public async Task EnterToChat(long chatId)
    {
        var groupId = await ValidateChatConnect(_chatService, chatId);
        if (groupId == null) return;
        
        await Groups.AddToGroupAsync(Context.ConnectionId, _cachedChatIds[chatId]);
    }

    [Authorize]
    public async Task Send(long chatId)
    {
        var groupId = await ValidateChatConnect(_chatService, chatId);
        if (groupId == null) return;

        //_eventBus.Invoke(new MessageSend() {From = user.Id, To = chatId, Message = "Test"});
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
            // todo event user offline to bus
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    
    private async Task<string?> ValidateChatConnect(IChatService service, long chatId)
    {
        var user = GetUserFromContext(HttpContext);
        if (user == null) return null;
        if (!await service.IsChatExist(chatId)) return null;
        var chatType = service.GetChatType(chatId);
        if (chatType == ChatType.Group && !await service.IsUserInChat(user.Id)) return null;

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