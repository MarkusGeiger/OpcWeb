using Microsoft.AspNetCore.Mvc;

namespace OpcWeb.Api;

[ApiController]
[Route("[controller]")]
public class ConnectionController : ControllerBase
{
  private readonly ConnectionHandler _handler;

  public ConnectionController(ConnectionHandler handler)
  {
    _handler = handler;
  }
  
  [HttpGet]
  public IActionResult GetAll()
  {
    return Ok(_handler.GetConnections());
  }

  [HttpPost]
  public IActionResult AddConnection([FromBody] OpcUaConnection connection)
  {
    var id = _handler.AddConnection(connection);
    return Ok(id);
  }

  [HttpPut]
  public IActionResult UpdateConnection([FromBody] OpcUaConnection connection)
  {
    var id = _handler.UpdateConnection(connection);
    return Ok(id);
  }
}