using Microsoft.EntityFrameworkCore;
using OnlineChatV2.WebApi.Infrastructure;

namespace OnlineChatV2.WebApi.Utilities;

public static class Extensions
{
    public static async Task<T> FirstOrError<T>(this IEnumerable<T> source, Func<T, bool> filter, string notFoundError)
    {
        return await Task.Run(() => 
            {
                var entity = source.FirstOrDefault(filter);
                return entity != null ? entity : throw new Exception(notFoundError);
            }
        );
    }
    
    public static async Task<T> FirstOrError<T, TError>(this IQueryable<T> source, Func<T, bool> filter, string notFoundError) where TError : Exception, new()
    {
        var entity = source.FirstOrDefault(filter);
        return entity != null ? entity : throw (Activator.CreateInstance(typeof(TError), notFoundError) as TError)!;
                
    }
    
    public static async Task<T> FirstOrError<T>(this IQueryable<T> source, Func<T, bool> filter, string notFoundError)
    {
        return await Task.Run(() => 
            {
                var entity = source.FirstOrDefault(filter);
                return entity != null ? entity : throw new Exception(notFoundError);
            }
        );
    }
    
    public static string GetEnumInfo(this Enum value)
    {
        var type = value.GetType();
        var memberInfo = type.GetMember(value.ToString());
        var attributes = memberInfo[0].GetCustomAttributes(typeof(EnumInfoAttribute), false);
        
        if (attributes.Length > 0 && attributes[0] is EnumInfoAttribute attribute)
        {
            return attribute.Name;
        }

        return value.ToString();
    }
}