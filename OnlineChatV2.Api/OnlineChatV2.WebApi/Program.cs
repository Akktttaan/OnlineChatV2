using OnlineChatV2.WebApi.Hubs;
using OnlineChatV2.WebApi.Hubs.EventManagement;
using OnlineChatV2.WebApi.Infrastructure.Middlewares;
using OnlineChatV2.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddConfiguration(builder.Configuration);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddAuth();
builder.Services.AddSingleton<EventBus>();
builder.Services.AddSignalR();

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
app.MapHub<ChatHub>("/chat");
app.MapHub<NotifyHub>("/notify");

app.Run();