using Microsoft.EntityFrameworkCore;
using OnlineChatV2.Domain;

namespace OnlineChatV2.Dal;

public class QueryDbContext : BaseDbContext
{
    public QueryDbContext(DbContextOptions<QueryDbContext> options) : base(options)
    {
        
    }
}