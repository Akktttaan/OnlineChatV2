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
    
    public static async Task<T> FirstOrError<T>(this IQueryable<T> source, Func<T, bool> filter, string notFoundError)
    {
        return await Task.Run(() => 
            {
                var entity = source.FirstOrDefault(filter);
                return entity != null ? entity : throw new Exception(notFoundError);
            }
        );
    }
}