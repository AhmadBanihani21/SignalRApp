using Microsoft.AspNetCore.SignalR;
using SignalRRoutingDemo.Hubs;
using SignalRRoutingDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DemoDataStore>();
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();
builder.Services.AddSingleton<IUserIdProvider, QueryStringUserIdProvider>();
builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials());
});

var app = builder.Build();

app.UseCors();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
