using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace OnlineChatV2.WebApi.Utilities;

public class DbUtilities
{
    public static List<TProp> RandomRows<T, TProp>(DbSet<T> dbSet, int count, Expression<Func<T, TProp>> selector) where T : class
    {
        var rand = new Random();  
        var skipper = rand.Next(0, dbSet.Count());
        return dbSet.Skip(skipper).Take(count).Select(selector).ToList();
    }  
}