namespace SignalRRoutingDemo.Models;

public class DemoGroup
{
    public string GroupId { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public List<string> MemberUserIds { get; set; } = new();
}
