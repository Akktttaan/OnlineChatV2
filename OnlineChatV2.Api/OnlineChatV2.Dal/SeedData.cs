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

        if (!context.NicknameColors.Any())
        {
            context.NicknameColors.AddRange(new []
            {
                new NicknameColor()
                {
                    Hex = "#8B0000"
                },
                new NicknameColor()
                {
                    Hex = "#FF4500"
                },
                new NicknameColor()
                {
                    Hex = "#8B8B00"
                },
                new NicknameColor()
                {
                    Hex = "#006400"
                },
                new NicknameColor()
                {
                    Hex = "#008B8B"
                },
                new NicknameColor()
                {
                    Hex = "#00008B"
                },
                new NicknameColor()
                {
                    Hex = "#8B008B"
                }
            });
        }

        context.SaveChanges();
    }
}