using SignalRRoutingDemo.Models;

namespace SignalRRoutingDemo.Services;

public class DemoDataStore
{
    public IReadOnlyList<DemoUser> Users { get; }
    public IReadOnlyList<DemoGroup> Groups { get; }

    public DemoDataStore()
    {
        Users = new List<DemoUser>
        {
            new()
            {
                UserId = "ahmad",
                UserName = "Ahmad",
                GroupIds = new List<string> { "developers", "devops" }
            },
            new()
            {
                UserId = "maen",
                UserName = "Maen",
                GroupIds = new List<string> { "quality" }
            },
            new()
            {
                UserId = "mostafa",
                UserName = "Mostafa",
                GroupIds = new List<string> { "developers" }
            }
        };

        Groups = new List<DemoGroup>
        {
            new()
            {
                GroupId = "developers",
                GroupName = "Developers Team",
                MemberUserIds = new List<string> { "ahmad", "mostafa" }
            },
            new()
            {
                GroupId = "quality",
                GroupName = "Quality Team",
                MemberUserIds = new List<string> { "maen" }
            },
            new()
            {
                GroupId = "devops",
                GroupName = "DevOps Team",
                MemberUserIds = new List<string> { "ahmad" }
            }
        };
    }

    public DemoUser? GetUser(string userId) =>
        Users.FirstOrDefault(u => u.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase));

    public DemoGroup? GetGroup(string groupId) =>
        Groups.FirstOrDefault(g => g.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<string> GetGroupIdsForUser(string userId)
    {
        var user = GetUser(userId);
        return user?.GroupIds ?? Enumerable.Empty<string>();
    }
}
