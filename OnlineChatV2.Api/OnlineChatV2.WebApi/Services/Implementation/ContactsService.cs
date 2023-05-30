using Microsoft.EntityFrameworkCore;
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
            .Select(x => new ContactModel() { UserId = x.Contact.Id, Username = x.Contact.Username })
            .ToArrayAsync();
    }

    public async Task RemoveContact(long userId, long contactId)
    {
        var user = await _commandDb.UsersContacts.FirstOrError(x => x.ContactOwnerId == userId && x.ContactId == contactId,
            "Контакт не найден");
        _commandDb.UsersContacts.Remove(user);
        await _commandDb.SaveChangesAsync();
    }

    public async Task<IEnumerable<ContactModel>> SearchUsers(string searchString)
    {
        return await _queryDb.Users
            .Where(u => EF.Functions.Like(u.Username.ToLower(), "%" + searchString.ToLower() + "%"))
            .Select(x => new ContactModel() { UserId = x.Id, Username = x.Username })
            .ToArrayAsync();
    }
}