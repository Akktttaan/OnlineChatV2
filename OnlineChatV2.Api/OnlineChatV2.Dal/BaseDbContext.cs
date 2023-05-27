using Microsoft.EntityFrameworkCore;
using OnlineChatV2.Domain;

namespace OnlineChatV2.Dal;

public class BaseDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    public BaseDbContext(DbContextOptions options) : base(options)
    {
        
    }
}