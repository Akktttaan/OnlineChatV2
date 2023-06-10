using OnlineChatV2.WebApi.Models;

namespace OnlineChatV2.WebApi.Services.Base;

public interface IContactsService
{
    Task AddContact(long userId, long contactId);
    Task<IEnumerable<ContactModel>> GetUserContacts(long userId);
    Task RemoveContact(long userId, long contactId);
    Task<IEnumerable<ContactModel>> SearchUsers(string searchString);
    Task<IEnumerable<ContactModel>> MissingChatUsers(long userId, long chatId);
}