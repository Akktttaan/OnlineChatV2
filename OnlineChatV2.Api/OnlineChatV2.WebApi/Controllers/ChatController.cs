using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OnlineChatV2.WebApi.Infrastructure;
using OnlineChatV2.WebApi.Models;
using OnlineChatV2.WebApi.Services.Base;

namespace OnlineChatV2.WebApi.Controllers;

[ApiController]
[Route("/api/chat/")]
[EnableCors("CorsPolicy")]
public class ChatController : ControllerBase
{
    [HttpGet("chatInfo")]
    [Authorize]
    public async Task<ChatInfo> GetChatInfo([FromServices]  IChatService chatService, long chatId)
    {
        return await chatService.GetChatInfo(chatId);
    }
}