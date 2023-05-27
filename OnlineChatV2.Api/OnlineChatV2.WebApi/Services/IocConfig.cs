using Microsoft.EntityFrameworkCore;
using OnlineChatV2.Dal;
using OnlineChatV2.WebApi.Services.Base;
using OnlineChatV2.WebApi.Services.Implementation;

namespace OnlineChatV2.WebApi.Services;

public static class IocConfig
{
    public static void AddConfiguration(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddSingleton(configuration);
    }

    public static void AddDatabase(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var readonlyConnString = configuration.GetConnectionString("ReadonlyConnectionString");
        var defaultConnString = configuration.GetConnectionString("DefaultConnectionString");
        serviceCollection.AddDbContext<QueryDbContext>(opt => opt.UseNpgsql(readonlyConnString));
        serviceCollection.AddDbContext<CommandDbContext>(opt => opt.UseNpgsql(defaultConnString));
    }

    public static void AddAuth(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IUserService, UserService>();
    }
}