using Microsoft.EntityFrameworkCore;
using OnlineChatV2.Dal;
using OnlineChatV2.Domain;
using OnlineChatV2.WebApi.Models;
using OnlineChatV2.WebApi.Services.Base;
using OnlineChatV2.WebApi.Utilities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace OnlineChatV2.WebApi.Services.Implementation;

public class UserService : IUserService
{
    private readonly QueryDbContext _queryDb;
    private readonly CommandDbContext _commandDb;
    private readonly IConfiguration _configuration;

    public UserService(QueryDbContext queryDb, CommandDbContext commandDb, IConfiguration configuration)
    {
        _queryDb = queryDb;
        _commandDb = commandDb;
        _configuration = configuration;
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
    
    public async Task<AuthenticateResponse?> Register(string username, string password, string email)
    {
        var users = _commandDb.Users;
        var exist = await users.FirstOrDefaultAsync(x => x.Username == username || x.Email == email);
        if (exist != null)
            return await Task.FromResult<AuthenticateResponse>(null);
        var hash = CryptoUtilities.GetMd5String(password);
        var user = new User()
        {
            Password = hash,
            Email = email,
            Username = username
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
        return await _queryDb.Users.Include(x => x.UserRoles).FirstOrDefaultAsync(x => x.Id == id);
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
        return await Task.Run(() => _queryDb.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).Select(x =>
            new UserViewModel()
            {
                Username = x.Username,
                Email = x.Email,
                Id = x.Id,
                IsAdmin = x.UserRoles.Any(x =>
                    string.Equals(x.Role.Name.ToLower(), "admin"))
            }).ToArray());
    }

}