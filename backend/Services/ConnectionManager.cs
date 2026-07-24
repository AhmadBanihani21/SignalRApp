using System.Collections.Concurrent;
using SignalRRoutingDemo.Models;

namespace SignalRRoutingDemo.Services;

public interface IConnectionManager
{
    void AddConnection(UserConnection connection);
    void RemoveConnection(string connectionId);
    UserConnection? GetByConnectionId(string connectionId);
    IReadOnlyList<UserConnection> GetAll();
    IReadOnlyList<UserConnection> GetByUserId(string userId);
    IReadOnlyList<UserConnection> GetByUserIds(IEnumerable<string> userIds);
    IReadOnlyList<UserConnection> GetByGroupId(string groupId);
    IReadOnlyList<UserConnection> GetByGroupIds(IEnumerable<string> groupIds);
}

public class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<string, UserConnection> _connections = new();
    private readonly DemoDataStore _demoData;

    public ConnectionManager(DemoDataStore demoData)
    {
        _demoData = demoData;
    }

    public void AddConnection(UserConnection connection)
    {
        _connections[connection.ConnectionId] = connection;
    }

    public void RemoveConnection(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }

    public UserConnection? GetByConnectionId(string connectionId)
    {
        _connections.TryGetValue(connectionId, out var connection);
        return connection;
    }

    public IReadOnlyList<UserConnection> GetAll() =>
        _connections.Values.OrderBy(c => c.UserName).ThenBy(c => c.BrowserName).ToList();

    public IReadOnlyList<UserConnection> GetByUserId(string userId) =>
        _connections.Values
            .Where(c => c.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
            .ToList();

    public IReadOnlyList<UserConnection> GetByUserIds(IEnumerable<string> userIds)
    {
        var set = new HashSet<string>(userIds, StringComparer.OrdinalIgnoreCase);
        return _connections.Values.Where(c => set.Contains(c.UserId)).ToList();
    }

    public IReadOnlyList<UserConnection> GetByGroupId(string groupId)
    {
        var group = _demoData.GetGroup(groupId);
        if (group is null) return Array.Empty<UserConnection>();
        return GetByUserIds(group.MemberUserIds);
    }

    public IReadOnlyList<UserConnection> GetByGroupIds(IEnumerable<string> groupIds)
    {
        var memberIds = groupIds
            .Select(id => _demoData.GetGroup(id))
            .Where(g => g is not null)
            .SelectMany(g => g!.MemberUserIds)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        return GetByUserIds(memberIds);
    }
}
