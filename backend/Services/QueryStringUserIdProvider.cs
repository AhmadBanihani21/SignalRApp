using Microsoft.AspNetCore.SignalR;

namespace SignalRRoutingDemo.Services;

public class QueryStringUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var http = connection.GetHttpContext();
        return http?.Request.Query["userId"].FirstOrDefault();
    }
}
