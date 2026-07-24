using Microsoft.AspNetCore.Mvc;
using SignalRRoutingDemo.Services;

namespace SignalRRoutingDemo.Controllers;

[ApiController]
[Route("api")]
public class DemoController : ControllerBase
{
    private readonly DemoDataStore _demoData;
    private readonly IConnectionManager _connections;

    public DemoController(DemoDataStore demoData, IConnectionManager connections)
    {
        _demoData = demoData;
        _connections = connections;
    }

    [HttpGet("users")]
    public IActionResult GetUsers() => Ok(_demoData.Users);

    [HttpGet("groups")]
    public IActionResult GetGroups() => Ok(_demoData.Groups);

    [HttpGet("connections")]
    public IActionResult GetConnections() => Ok(_connections.GetAll());
}
