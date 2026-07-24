namespace SignalRRoutingDemo.Models;

public class RoutedMessage
{
    public string Method { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string SenderConnectionId { get; set; } = string.Empty;
    public string SenderUserId { get; set; } = string.Empty;
    public string SenderUserName { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public List<string> IntendedConnectionIds { get; set; } = new();
    public string TargetingDescription { get; set; } = string.Empty;
}

public class ConnectionsSnapshot
{
    public List<UserConnection> Connections { get; set; } = new();
}
