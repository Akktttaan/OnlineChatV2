using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OnlineChatV2.WebApi.Infrastructure;
using OnlineChatV2.WebApi.Models;
using OnlineChatV2.WebApi.Services.Base;

namespace OnlineChatV2.WebApi.Controllers;

[ApiController]
[Route("/api/profile/")]
[EnableCors("CorsPolicy")]
public class ProfileController : ControllerBase
{
    private readonly IUserService _userService;

    
    public ProfileController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPut("updateAbout")]
    [SameUserCheck]
    [Authorize]
    public async Task<IActionResult> UpdateAbout(UpdateAboutDto dto)
    {
        try
        {
            await _userService.UpdateAbout(dto.UserId, dto.About);
            return Ok();
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPut("uploadPhoto")]
    [SameUserCheck]
    [Authorize]
    public async Task<IActionResult> UplaodPhoto(UploadPhotoDto dto, [FromServices] IWebHostEnvironment env)
    {
        try
        {
            await _userService.UploadPhoto(dto.UserId, dto.Photo, env);
            return Ok();
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
        catch (Exception e)
        {
            return Problem(e.Message);
        }
    }
    
}