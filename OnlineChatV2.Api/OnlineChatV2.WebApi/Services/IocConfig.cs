using Microsoft.EntityFrameworkCore;
using OnlineChatV2.Dal;
using OnlineChatV2.WebApi.Services.Base;
using OnlineChatV2.WebApi.Services.Implementation;

namespace OnlineChatV2.WebApi.Services;

public static class IocConfig
{
    public static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddSingleton(configuration);

        return serviceCollection;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var readonlyConnString = configuration.GetConnectionString("ReadonlyConnectionString");
        var defaultConnString = configuration.GetConnectionString("DefaultConnectionString");
        serviceCollection.AddDbContext<QueryDbContext>(opt => opt.UseNpgsql(readonlyConnString));
        serviceCollection.AddDbContext<CommandDbContext>(opt => opt.UseNpgsql(defaultConnString));

        return serviceCollection;
    }

    public static IServiceCollection AddAuth(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IUserService, UserService>();

        return serviceCollection;
    }
    
    /// <summary>
    /// Метод расширения добавляющий CORS
    /// </summary>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options => options.AddPolicy("CorsPolicy",
            builder => builder.SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("Content-Disposition")
                .AllowCredentials()));

        return services;
    }
}