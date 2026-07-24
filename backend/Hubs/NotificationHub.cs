using Microsoft.AspNetCore.SignalR;
using SignalRRoutingDemo.Models;
using SignalRRoutingDemo.Services;

namespace SignalRRoutingDemo.Hubs;

public class NotificationHub : Hub
{
    private readonly IConnectionManager _connections;
    private readonly DemoDataStore _demoData;
    private readonly ILogger<NotificationHub> _logger;

    public const string ReceiveRoutedMessage = "ReceiveRoutedMessage";
    public const string RoutingAnalysis = "RoutingAnalysis";
    public const string ConnectionsChanged = "ConnectionsChanged";

    public NotificationHub(
        IConnectionManager connections,
        DemoDataStore demoData,
        ILogger<NotificationHub> logger)
    {
        _connections = connections;
        _demoData = demoData;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var http = Context.GetHttpContext();
        var userId = http?.Request.Query["userId"].FirstOrDefault() ?? "unknown";
        var browserName = http?.Request.Query["browser"].FirstOrDefault() ?? "Unknown Browser";

        var user = _demoData.GetUser(userId);
        var userName = user?.UserName ?? userId;

        var connection = new UserConnection
        {
            UserId = userId,
            UserName = userName,
            ConnectionId = Context.ConnectionId,
            BrowserName = browserName,
            ConnectedAt = DateTime.UtcNow
        };

        _connections.AddConnection(connection);

        foreach (var groupId in _demoData.GetGroupIdsForUser(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
            _logger.LogInformation(
                "Connection {ConnectionId} ({User}) joined group {Group}",
                Context.ConnectionId, userName, groupId);
        }

        _logger.LogInformation(
            "Connected: {User} via {Browser} → ConnectionId={ConnectionId}",
            userName, browserName, Context.ConnectionId);

        await BroadcastConnectionsChanged();
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connections.RemoveConnection(Context.ConnectionId);
        _logger.LogInformation("Disconnected: {ConnectionId}", Context.ConnectionId);

        await BroadcastConnectionsChanged();
        await base.OnDisconnectedAsync(exception);
    }

    private async Task BroadcastConnectionsChanged()
    {
        var snapshot = new ConnectionsSnapshot { Connections = _connections.GetAll().ToList() };
        await Clients.All.SendAsync(ConnectionsChanged, snapshot);
    }

    private RoutedMessage BuildPayload(
        string method,
        string message,
        IEnumerable<UserConnection> intended,
        string targetingDescription)
    {
        var sender = _connections.GetByConnectionId(Context.ConnectionId);
        return new RoutedMessage
        {
            Method = method,
            Message = message,
            SenderConnectionId = Context.ConnectionId,
            SenderUserId = sender?.UserId ?? "",
            SenderUserName = sender?.UserName ?? "Unknown",
            SentAt = DateTime.UtcNow,
            IntendedConnectionIds = intended.Select(c => c.ConnectionId).ToList(),
            TargetingDescription = targetingDescription
        };
    }

    private Task NotifyCallerAnalysis(RoutedMessage payload) =>
        Clients.Caller.SendAsync(RoutingAnalysis, payload);

    public async Task SendToAll(string message)
    {
        var intended = _connections.GetAll();
        var payload = BuildPayload(
            "Clients.All",
            message,
            intended,
            "Broadcast to every connected client on the hub.");

        await NotifyCallerAnalysis(payload);
        await Clients.All.SendAsync(ReceiveRoutedMessage, payload);
    }

    public async Task SendToCaller(string message)
    {
        var intended = _connections.GetByConnectionId(Context.ConnectionId) is { } self
            ? new[] { self }
            : Array.Empty<UserConnection>();

        var payload = BuildPayload(
            "Clients.Caller",
            message,
            intended,
            "Only the calling connection receives this message.");

        await NotifyCallerAnalysis(payload);
        await Clients.Caller.SendAsync(ReceiveRoutedMessage, payload);
    }

    public async Task SendToOthers(string message)
    {
        var intended = _connections.GetAll()
            .Where(c => c.ConnectionId != Context.ConnectionId)
            .ToList();

        var payload = BuildPayload(
            "Clients.Others",
            message,
            intended,
            "All connections except the caller.");

        await NotifyCallerAnalysis(payload);
        await Clients.Others.SendAsync(ReceiveRoutedMessage, payload);
    }

    public async Task SendToConnection(string message, string connectionId)
    {
        var intended = _connections.GetByConnectionId(connectionId) is { } target
            ? new[] { target }
            : Array.Empty<UserConnection>();

        var payload = BuildPayload(
            $"Clients.Client(\"{Short(connectionId)}\")",
            message,
            intended,
            $"Only the single connection with ConnectionId = {connectionId}.");

        await NotifyCallerAnalysis(payload);
        await Clients.Client(connectionId).SendAsync(ReceiveRoutedMessage, payload);
    }

    public async Task SendToConnections(string message, List<string> connectionIds)
    {
        var intended = connectionIds
            .Select(id => _connections.GetByConnectionId(id))
            .Where(c => c is not null)
            .Cast<UserConnection>()
            .ToList();

        var shortIds = string.Join(", ", connectionIds.Select(Short));
        var payload = BuildPayload(
            $"Clients.Clients([{shortIds}])",
            message,
            intended,
            $"Explicit list of ConnectionIds: {string.Join(", ", connectionIds)}.");

        await NotifyCallerAnalysis(payload);
        await Clients.Clients(connectionIds).SendAsync(ReceiveRoutedMessage, payload);
    }

    public async Task SendToUser(string message, string userId)
    {
        var intended = _connections.GetByUserId(userId);
        var name = _demoData.GetUser(userId)?.UserName ?? userId;

        var payload = BuildPayload(
            $"Clients.User(\"{userId}\")",
            message,
            intended,
            $"All connections for user {name} (User ≠ single Connection).");

        await NotifyCallerAnalysis(payload);
        await Clients.User(userId).SendAsync(ReceiveRoutedMessage, payload);
    }

    public async Task SendToUsers(string message, List<string> userIds)
    {
        var intended = _connections.GetByUserIds(userIds);
        var names = string.Join(", ", userIds);

        var payload = BuildPayload(
            $"Clients.Users([{names}])",
            message,
            intended,
            $"All connections for users: {names}.");

        await NotifyCallerAnalysis(payload);
        await Clients.Users(userIds).SendAsync(ReceiveRoutedMessage, payload);
    }

    public async Task SendToGroup(string message, string groupId)
    {
        var intended = _connections.GetByGroupId(groupId);
        var groupName = _demoData.GetGroup(groupId)?.GroupName ?? groupId;

        var payload = BuildPayload(
            $"Clients.Group(\"{groupId}\")",
            message,
            intended,
            $"All connections currently in group '{groupName}'.");

        await NotifyCallerAnalysis(payload);
        await Clients.Group(groupId).SendAsync(ReceiveRoutedMessage, payload);
    }

    public async Task SendToGroups(string message, List<string> groupIds)
    {
        var intended = _connections.GetByGroupIds(groupIds);
        var names = string.Join(", ", groupIds);

        var payload = BuildPayload(
            $"Clients.Groups([{names}])",
            message,
            intended,
            $"Union of connections in groups: {names}.");

        await NotifyCallerAnalysis(payload);
        await Clients.Groups(groupIds).SendAsync(ReceiveRoutedMessage, payload);
    }

    public async Task SendToGroupExcept(string message, string groupId, List<string> excludedConnectionIds)
    {
        var intended = _connections.GetByGroupId(groupId)
            .Where(c => !excludedConnectionIds.Contains(c.ConnectionId, StringComparer.Ordinal))
            .ToList();

        var groupName = _demoData.GetGroup(groupId)?.GroupName ?? groupId;
        var excluded = string.Join(", ", excludedConnectionIds.Select(Short));

        var payload = BuildPayload(
            $"Clients.GroupExcept(\"{groupId}\", [{excluded}])",
            message,
            intended,
            $"Group '{groupName}' minus excluded connection(s): {excluded}.");

        await NotifyCallerAnalysis(payload);
        await Clients.GroupExcept(groupId, excludedConnectionIds)
            .SendAsync(ReceiveRoutedMessage, payload);
    }

    private static string Short(string connectionId) =>
        connectionId.Length <= 8 ? connectionId : connectionId[..8] + "…";
}
