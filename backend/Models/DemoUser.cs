namespace SignalRRoutingDemo.Models;

public class DemoUser
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<string> GroupIds { get; set; } = new();
}
