using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OnlineChatV2.WebApi.Infrastructure;
using OnlineChatV2.WebApi.Models;
using OnlineChatV2.WebApi.Services.Base;

namespace OnlineChatV2.WebApi.Controllers;

[ApiController]
[Route("/api/user/")]
[EnableCors("CorsPolicy")]
public class AuthController : ControllerBase
{
    private readonly IUserService _authService;

    public AuthController(IUserService authService)
    {
        _authService = authService;
    }

    [HttpGet("all")]
    [Authorize("Admin")]
    public async Task<UserViewModel[]> GetUsers()
    {
        return await _authService.GetAllUsers();
    }
    
    [Authorize("Admin")]
    [HttpPut("promoteUser")]
    public async Task<IActionResult> PromoteToAdmin([FromBody] long userId)
    {
        try
        {
            await _authService.PromoteToAdmin(userId);
            return Ok();
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [Authorize]
    [HttpGet("verify")]
    [ProducesResponseType(200)]
    public IActionResult VerifyToken()
    {
        return Ok();
    }

    [HttpPost("auth")]
    [ProducesResponseType(typeof(AuthenticateResponse),200)]
    public async Task<IActionResult> Auth([FromBody] LoginDto dto)
    {
        var response = await _authService.Auth(dto.NameOrEmail, dto.Password);
        if (response == null)
        {
            return BadRequest("Логин или пароль неправильный");
        }

        return Ok(response);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticateResponse),200)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var response = await _authService.Register(dto.Username, dto.Password, dto.Email);
        if (response == null)
        {
            return BadRequest("Данный email или username уже используется");
        }

        return Ok(response);
    }
}