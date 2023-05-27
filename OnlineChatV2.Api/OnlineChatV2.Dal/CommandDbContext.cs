using Microsoft.EntityFrameworkCore;

namespace OnlineChatV2.Dal;

public class CommandDbContext : BaseDbContext
{
    public CommandDbContext(DbContextOptions<CommandDbContext> options) : base(options)
    {
    }
}