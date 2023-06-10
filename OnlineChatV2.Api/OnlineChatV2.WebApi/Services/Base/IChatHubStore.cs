using Swashbuckle.AspNetCore.SwaggerGen;

namespace OnlineChatV2.WebApi.Services.Base;

public interface IChatHubStore
{ 
    void AddChat(long chatId, string groupName);
    void RemoveChat(long chatId);
    void AddUser(string connectionId, long userId);
    void AddGroup(long chatId, string groupName);
    void AddUserToGroup(string connectionId, string groupName);
    void RemoveUserFromGroup(long userId, string groupName);
    bool GroupExist(string groupName);
    bool TryGetGroupId(long chatId, out string? groupId);
    bool UserInGroup(string groupName, string connectionId);
    bool UserInCache(string connectionId);
    string? GetUserConnectionId(long userId);
    IEnumerable<string> GetGroupMembers(string groupName);
    void RemoveUser(string connectionId);
    void RemoveUserFromGroups(string connectionId);
    long? GetUserId(string connectionId);
    IEnumerable<string> GetGroups();
    IEnumerable<long> GetOnlineUsers();
    IEnumerable<string> GetOnlineClients();
}