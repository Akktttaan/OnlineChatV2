using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineChatV2.Domain;

namespace OnlineChatV2.Dal;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<CommandDbContext>();

        // Применяем все миграции, которых еще нет в базе данных
        context.Database.Migrate();
        
        FillRoles(context);
    }

    private static void FillRoles(BaseDbContext context)
    {
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(new []
            {
                new Role
                {
                    Name = "Admin",
                    IsDefault = false
                },
                new Role
                {
                    Name = "User",
                    IsDefault = true
                }
            });
        }

        context.SaveChanges();
    }
}