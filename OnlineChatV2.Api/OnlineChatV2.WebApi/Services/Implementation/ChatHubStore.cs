using System.Collections.Concurrent;
using OnlineChatV2.WebApi.Services.Base;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OnlineChatV2.WebApi.Services.Implementation;

public class ChatHubStore : IChatHubStore
{
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
    private readonly ConcurrentDictionary<string, List<string>> _groups;
    
    public ChatHubStore()
    {
        _cachedChatIds = new();
        _cachedUsers = new();
        _groups = new();
    }
    
    public void AddChat(long chatId, string groupName)
    {
        if (_cachedChatIds.ContainsKey(chatId))
            return;
        _cachedChatIds[chatId] = groupName;
    }

    public void RemoveChat(long chatId)
    {
        if (!_cachedChatIds.ContainsKey(chatId))
            return;
        _cachedChatIds.TryRemove(chatId, out _);
    }

    public void AddUser(string connectionId, long userId)
    {
        if (_cachedUsers.ContainsKey(connectionId))
            return;
        _cachedUsers[connectionId] = userId;
    }

    public void AddGroup(long chatId, string groupName)
    {
        if(!_cachedChatIds.ContainsKey(chatId))
            _cachedChatIds[chatId] = groupName;
        AddGroup(groupName);
    }

    private void AddGroup(string groupName)
    {
        if (_groups.ContainsKey(groupName))
            return;
        _groups[groupName] = new List<string>();
    }
    
    public void AddUserToGroup(string connectionId, string groupName)
    {
        if(!_groups.ContainsKey(groupName))
            AddGroup(groupName);
        if (_groups[groupName].Contains(connectionId))
            return;
        _groups[groupName].Add(connectionId);
    }

    public void RemoveUserFromGroup(long userId, string groupName)
    {
        if (_groups.IsEmpty)
            return;
        if (_groups[groupName].Count == 0)
            return;
        var connectionId = GetUserConnectionId(userId);
        if (connectionId == null)
            return;
        _groups[groupName].Remove(connectionId);
    }

    public bool GroupExist(string groupName) => _groups.ContainsKey(groupName);

    public bool UserInGroup(string groupName, string connectionId) =>
        _groups.IsEmpty && _groups[groupName].Contains(connectionId);

    public bool UserInCache(string connectionId) => _cachedUsers.ContainsKey(connectionId);

    public string? GetUserConnectionId(long userId)
    {
        return _cachedUsers.FirstOrDefault(x => x.Value == userId).Key;
    }

    public IEnumerable<string> GetGroupMembers(string groupName)
    {
        return _groups[groupName];
    }

    public void RemoveUser(string connectionId)
    {
        if (UserInCache(connectionId))
            _cachedUsers.TryRemove(connectionId, out _);
    }

    public void RemoveUserFromGroups(string connectionId)
    {
        foreach (var groupMembers in _groups.Values)
        {
            groupMembers.Remove(connectionId);
        }
    }

    public bool TryGetGroupId(long chatId, out string? groupId)
    {
        return _cachedChatIds.TryGetValue(chatId, out groupId);
    }

    public long? GetUserId(string connectionId)
    {
        if (_cachedUsers.TryGetValue(connectionId, out var id))
            return id;
        return null;
    }

    public IEnumerable<string> GetGroups() => _groups.Keys;
    public IEnumerable<long> GetOnlineUsers() => _cachedUsers.Values;
    public IEnumerable<string> GetOnlineClients() => _cachedUsers.Keys;

}