using Microsoft.AspNetCore.SignalR;
using OnlineChatV2.Dal;
using OnlineChatV2.WebApi.Hubs;
using OnlineChatV2.WebApi.Hubs.EventManagement;
using OnlineChatV2.WebApi.Infrastructure;
using OnlineChatV2.WebApi.Infrastructure.Middlewares;
using OnlineChatV2.WebApi.Services;
using OnlineChatV2.WebApi.Services.Base;
using OnlineChatV2.WebApi.Services.Implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddConfiguration(builder.Configuration)
    .AddDatabase(builder.Configuration)
    .AddAuth()
    .AddSingleton<EventBus>()
    .AddTransient<IChatService, ChatService>()
    .AddScoped<IContactsService, ContactsService>()
    .AddSingleton<IChatHubStore, ChatHubStore>()
    .AddSingleton<IFileService, FileService>()
    .AddCorsPolicy()
    .AddSignalR(opt => opt.AddFilter<AuthHubFilter>())
    .AddHubOptions<ChatHub>(options =>
    {
        options.AddFilter<AuthHubFilter>();
    });


var app = builder.Build();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<JwtMiddleware>();
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();
app.UseCors("CorsPolicy");
app.MapHub<ChatHub>("/chat");
app.MapHub<NotifyHub>("/notify");

using var scope = app.Services.CreateScope();
SeedData.Initialize(scope.ServiceProvider);

app.Run();