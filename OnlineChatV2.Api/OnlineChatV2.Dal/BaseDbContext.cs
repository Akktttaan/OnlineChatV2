using Microsoft.EntityFrameworkCore;
using OnlineChatV2.Domain;

namespace OnlineChatV2.Dal;

public class BaseDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<ChatUser> ChatUsers { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<ReadMessage> ReadMessages { get; set; }
    public DbSet<UserContact> UsersContacts { get; set; }
    public DbSet<NicknameColor> NicknameColors { get; set; }

    public BaseDbContext(DbContextOptions options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<long>("ChatIds").StartsAt(-1).IncrementsBy(-1);
        modelBuilder.Entity<Chat>().Property(x => x.Id).HasDefaultValueSql("nextval('\"ChatIds\"')");
        modelBuilder.Entity<ChatUser>()
            .HasIndex(cu => new { cu.UserId, cu.ChatId })
            .IsUnique(false)
            .HasName("IX_ChatUsers_UserId_ChatId");
    }
}