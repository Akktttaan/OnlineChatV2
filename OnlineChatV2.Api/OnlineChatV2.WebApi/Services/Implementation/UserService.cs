using Microsoft.EntityFrameworkCore;
using OnlineChatV2.Dal;
using OnlineChatV2.Domain;
using OnlineChatV2.WebApi.Models;
using OnlineChatV2.WebApi.Services.Base;
using OnlineChatV2.WebApi.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace OnlineChatV2.WebApi.Services.Implementation;

public class UserService : IUserService
{
    private readonly QueryDbContext _queryDb;
    private readonly CommandDbContext _commandDb;
    private readonly IConfiguration _configuration;
    private readonly IFileService _fileService;

    public UserService(QueryDbContext queryDb, CommandDbContext commandDb, 
        IConfiguration configuration,
        IFileService fileService)
    {
        _queryDb = queryDb;
        _commandDb = commandDb;
        _configuration = configuration;
        _fileService = fileService;
    }
    
    public async Task<AuthenticateResponse?> Auth(string nameOrEmail, string password)
    {
        var hash = CryptoUtilities.GetMd5String(password);
        var user = await _queryDb.Users.FirstOrDefaultAsync(x =>
            x.Password == hash && (x.Email == nameOrEmail || x.Username == nameOrEmail));
        return user == null
            ? await Task.FromResult<AuthenticateResponse>(null)
            : new AuthenticateResponse(user, GenerateJwt(user));
    }
    
    public async Task<AuthenticateResponse?> Register(RegisterDto dto)
    {
        var users = _commandDb.Users;
        var exist = await users.FirstOrDefaultAsync(x => x.Username == dto.Username || x.Email == dto.Email);
        if (exist != null)
            return await Task.FromResult<AuthenticateResponse>(null);
        var hash = CryptoUtilities.GetMd5String(dto.Password);
        var color = _queryDb.NicknameColors.OrderBy(x => Guid.NewGuid()).Take(1).First();
        var user = new User()
        {
            Password = hash,
            Email = dto.Email,
            Username = dto.Username,
            NicknameColor = color.Hex,
        };
        await users.AddAsync(user);
        await _commandDb.SaveChangesAsync();
        var roles = await _commandDb.Roles.Where(x => x.IsDefault)
            .Select(x => new UserRole { UserId = user.Id, RoleId = x.Id }).ToListAsync();
        await _commandDb.UserRoles.AddRangeAsync(roles);
        await _commandDb.SaveChangesAsync();
        return new AuthenticateResponse(user, GenerateJwt(user));
    }

    public string GenerateJwt(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["SecretKey"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<User?> GetUserById(long id)
    {
        return await _queryDb.Users
            .Include(x => x.UserRoles)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task PromoteToAdmin(long id)
    {
        var user = await _commandDb.Users.FirstOrError(x => x.Id == id, "Пользователь не найден");
        var role = await _commandDb.Roles.FirstOrError(x =>
            string.Equals(x.Name.ToLower(), "admin"), "Роль не найдена. Добавьте ее");
        var userRole = await _commandDb.UserRoles.FirstOrDefaultAsync(x => x.UserId != user.Id && x.RoleId != role.Id);
        if (userRole != null) return;
        await _commandDb.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = role.Id });
        await _commandDb.SaveChangesAsync();
    }

    public async Task<UserViewModel[]> GetAllUsers()
    {
        return await Task.Run(() => _queryDb.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Select(x =>
            new UserViewModel()
            {
                Username = x.Username,
                Email = x.Email,
                Id = x.Id,
                IsAdmin = x.UserRoles.Any(x =>
                    string.Equals(x.Role.Name.ToLower(), "admin"))
            }).ToArray());
    }

    public async Task<DateTime> UpdateLastSeenTime(long userId)
    {
        var user = await _commandDb.Users.FindAsync(userId);
        if (user == null)
            return new DateTime();
        user.WasOnline = DateTime.UtcNow;
        _commandDb.Users.Update(user);
        await _commandDb.SaveChangesAsync();
        return user.WasOnline;
    }

    public async Task<UserInfo> GetUserInfo(long userId)
    {
        var user = await _queryDb.Users.Include(x => x.UserAvatars)
                                        .FirstOrError<User, ArgumentException>(x => x.Id == userId,
            "Пользователь не найден");
        return new UserInfo()
        {
            Id = user.Id,
            Username = user.Username,
            About = user.About,
            LastSeen = user.WasOnline,
            AvatarUrl = user.CurrentAvatar,
            Avatars = user.UserAvatars.Select(x => x.AvatarUrl)
        };
    }

    public async Task UpdateAbout(long userId, string about)
    {
        var user = await _queryDb.Users.FirstOrError<User, ArgumentException>(x => x.Id == userId,
            "Пользователь не найден");
        user.About = about;
        _commandDb.Users.Update(user);
        await _commandDb.SaveChangesAsync();
    }

    public async Task UploadPhoto(long userId, FileModel photo, IWebHostEnvironment env)
    {
        var user = await _queryDb.Users.FirstOrError<User, ArgumentException>(x => x.Id == userId,
            "Пользователь не найден");
        var avatarPath = await _fileService.UploadAvatar(userId, photo, env.WebRootPath, AvatarType.User);
        var avatar = new UserAvatar
        {
            AvatarUrl = avatarPath,
            UserId = userId
        };
        await _commandDb.UserAvatars.AddAsync(avatar);
        await _commandDb.SaveChangesAsync();
        user.CurrentAvatar = avatar.AvatarUrl;
        _commandDb.Users.Update(user);
        await _commandDb.SaveChangesAsync();
    }
}