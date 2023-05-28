using OnlineChatV2.Dal;
using OnlineChatV2.WebApi.Hubs;
using OnlineChatV2.WebApi.Hubs.EventManagement;
using OnlineChatV2.WebApi.Infrastructure.Middlewares;
using OnlineChatV2.WebApi.Services;

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
    .AddCorsPolicy()
    .AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<JwtMiddleware>();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseCors("CorsPolicy");
app.MapHub<ChatHub>("/chat");
app.MapHub<NotifyHub>("/notify");

using var scope = app.Services.CreateScope();
SeedData.Initialize(scope.ServiceProvider);

app.Run();