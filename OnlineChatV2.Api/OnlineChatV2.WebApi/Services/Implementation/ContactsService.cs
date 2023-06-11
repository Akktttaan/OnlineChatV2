using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using OnlineChatV2.Dal;
using OnlineChatV2.Domain;
using OnlineChatV2.WebApi.Models;
using OnlineChatV2.WebApi.Services.Base;
using OnlineChatV2.WebApi.Utilities;

namespace OnlineChatV2.WebApi.Services.Implementation;

public class ContactsService : IContactsService
{
    private readonly CommandDbContext _commandDb;
    private readonly QueryDbContext _queryDb;

    public ContactsService(CommandDbContext commandDb, QueryDbContext queryDb)
    {
        _commandDb = commandDb;
        _queryDb = queryDb;
    }

    public async Task AddContact(long userId, long contactId)
    {
        var user = await _commandDb.UsersContacts.FirstOrDefaultAsync(x =>
            x.ContactOwnerId == userId && x.ContactId == contactId);
        if (user != null)
            throw new InvalidOperationException("Контакт уже добавлен");
        var contact = new UserContact()
        {
            ContactId = contactId,
            ContactOwnerId = userId
        };
        await _commandDb.UsersContacts.AddAsync(contact);
        await _commandDb.SaveChangesAsync();
    }

    public async Task<IEnumerable<ContactModel>> GetUserContacts(long userId)
    {
        return await _queryDb.UsersContacts.Where(x => x.ContactOwnerId == userId)
            .Include(x => x.Contact)
            .Select(x => new ContactModel() { UserId = x.Contact.Id, UserName = x.Contact.Username, PhotoUrl = x.Contact.CurrentAvatar})
            .ToArrayAsync();
    }

    public async Task RemoveContact(long userId, long contactId)
    {
        var user = await _commandDb.UsersContacts.FirstOrError(
            x => x.ContactOwnerId == userId && x.ContactId == contactId,
            "Контакт не найден");
        _commandDb.UsersContacts.Remove(user);
        await _commandDb.SaveChangesAsync();
    }

    public async Task<IEnumerable<ContactModel>> SearchUsers(string searchString)
    {
        return await _queryDb.Users
            .Where(u => EF.Functions.Like(u.Username.ToLower(), "%" + searchString.ToLower() + "%"))
            .Select(x => new ContactModel() { UserId = x.Id, UserName = x.Username, PhotoUrl = x.CurrentAvatar})
            .ToArrayAsync();
    }

    public async Task<IEnumerable<ContactModel>> MissingChatUsers(long userId, long chatId)
    {
        var chatUsers = _queryDb.ChatUsers.Where(x => x.ChatId == chatId);
        var contacts = _queryDb.UsersContacts
            .Where(x => x.ContactOwnerId == userId)
            .Include(x => x.Contact)
            .ThenInclude(x => x.UserAvatars);
        return await (from contact in contacts
                join chatUser in chatUsers on contact.ContactId equals chatUser.UserId into gj
                from subUsers in gj.DefaultIfEmpty()
                where subUsers == null
                select new ContactModel
                    { UserName = contact.Contact.Username, UserId = contact.Contact.Id, PhotoUrl = contact.Contact.CurrentAvatar })
            .ToArrayAsync();

    }

}