using OpcUaClient;

namespace OpcWeb.Api;

public class ConnectionHandler
{
  private readonly ILogger<ConnectionHandler> _logger;
  private List<OpcUaConnection> _connections = new();

  public ConnectionHandler(ILogger<ConnectionHandler> logger)
  {
    _logger = logger;
  }

  public int AddConnection(OpcUaConnection connection)
  {
    connection.Id = _connections.Count;
    _connections.Add(connection);
    connection.Connect();
    return connection.Id;
  }

  public int UpdateConnection(OpcUaConnection connection)
  {
    connection.Client = _connections[connection.Id].Client;
    _connections[connection.Id] = connection;
    _connections[connection.Id].Connect();
    return connection.Id;
  }

  public List<OpcUaConnection> GetConnections()
  {
    return _connections;
  }
}