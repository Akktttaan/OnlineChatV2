using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineChatV2.Domain;
using OnlineChatV2.WebApi.Hubs.EventManagement;
using OnlineChatV2.WebApi.Hubs.EventManagement.EventsAndReceivers;

namespace OnlineChatV2.WebApi.Hubs;

public class NotifyHub : BaseChatHub
{
    private ConcurrentDictionary<long, User> _onlineUsers;
    private ConcurrentDictionary<string, long> _userIdToConnection;
    public NotifyHub([FromServices] EventBus eventBus)
    {
        _onlineUsers = new ConcurrentDictionary<long, User>();
        _userIdToConnection = new ConcurrentDictionary<string, long>();
        eventBus.Register(new NotifyHubReceiver(OnMessageSend));
    }

    private void OnMessageSend(MessageSend @event)
    {
        Console.WriteLine($"Event from ChatHub: From {@event.From} to {@event.To}. Message: {@event.Message}");
    }

    public override async Task OnConnectedAsync()
    {
        var user = GetUserFromContext(HttpContext);
        if (user == null) return;
        AttachUserToHub(user);
        await Clients.All.SendAsync("userConnected", user.Id);
        await Clients.Caller.SendAsync("receiveOnlineUsers", _userIdToConnection.Keys.ToList());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_userIdToConnection.TryGetValue(Context.ConnectionId, out var userId))
            await Clients.All.SendAsync("userDisconnected", userId);
        DetachUserFromHub(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    private void AttachUserToHub(User user)
    {
        if (!_onlineUsers.ContainsKey(user.Id))
            _onlineUsers[user.Id] = user;
        if (!_userIdToConnection.ContainsKey(Context.ConnectionId))
            _userIdToConnection[Context.ConnectionId] = user.Id;
    }

    private void DetachUserFromHub(string connectionId)
    {
        if (!_userIdToConnection.TryGetValue(connectionId, out var userId)) return;
        _userIdToConnection.TryRemove(connectionId, out _);
        if (!_onlineUsers.TryGetValue(userId, out _)) return;
        _onlineUsers.TryRemove(userId, out _);
    }
}