using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OnlineChatV2.WebApi.Infrastructure;
using OnlineChatV2.WebApi.Models;
using OnlineChatV2.WebApi.Services.Base;

namespace OnlineChatV2.WebApi.Controllers;

[ApiController]
[Route("/api/contacts/")]
[EnableCors("CorsPolicy")]
public class FriendsController : ControllerBase
{
    private readonly IContactsService _contactsService;

    public FriendsController(IContactsService contactsService)
    {
        _contactsService = contactsService;
    }

    [HttpPost("add")]
    [SameUserCheck]
    [Authorize]
    [ProducesResponseType(200)]
    public async Task<IActionResult> AddContact(long userId, long contactId)
    {
        try
        {
            await _contactsService.AddContact(userId, contactId);
            return Ok();
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("delete")]
    [SameUserCheck]
    [Authorize]
    public async Task<IActionResult> DeleteContact(long userId, long contactId)
    {
        try
        {
            await _contactsService.AddContact(userId, contactId);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("all")]
    [SameUserCheck]
    [Authorize]
    [ProducesResponseType(typeof(ContactModel[]), 200)]
    public async Task<IEnumerable<ContactModel>> GetContacts(long userId)
    {
        return await _contactsService.GetUserContacts(userId);
    }
}