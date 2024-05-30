using Opc.Ua;
using OpcUaClient;
using OpcUaServer;

namespace OpcUaTests;

[TestClass]
public class ServerApplicationTests
{
  private const string DiscoveryUrl = "opc.tcp://localhost:4841/";
  
  [TestMethod]
  public void CreateInstanceSuccess()
  {
    var server = new SimpleServer("TestInstance");
    Assert.IsNotNull(server);
  }

  [TestMethod]
  public void GetStatusApplicationNotStarted()
  {
    var server = new SimpleServer("TestInstance");
    Assert.IsNotNull(server);
    
    Assert.IsTrue(server.IsRunning() == StatusCodes.BadServerHalted, $"Server status code {server.IsRunning()} was unexpected.");
  }

  [TestMethod]
  [ExpectedException(typeof(ServiceResultException))]
  public void GetStatusApplicationNotStartedException()
  {
    var server = new SimpleServer("TestInstance");
    Assert.IsNotNull(server);
    
    Assert.IsTrue(server.IsRunning() == StatusCodes.BadServerHalted, $"Server status code {server.IsRunning()} was unexpected.");
    
    Console.WriteLine(server.GetStatus());
  }

  [TestMethod]
  public void StartApplicationSuccess()
  {
    var server = new SimpleServer("TestInstance");
    Assert.IsNotNull(server);

    server.Start();
    
    Assert.IsTrue(server.IsRunning() == StatusCodes.BadServerHalted, $"Server status code {server.IsRunning()} was unexpected.");
    
    Console.WriteLine($"StartTime={server.GetStatus().StartTime} State={server.GetStatus().State}");
    var endpoints = server.GetEndpoints();
    Console.WriteLine($"{endpoints.Count} available Endpoints.");
    foreach (var endpoint in endpoints)
    {
      Console.WriteLine($"Available Endpoint: ${endpoint.EndpointUrl} - ${endpoint.SecurityPolicyUri}");
    }
  }

  [TestMethod]
  public void StartApplicationAndConnectClient()
  {
    var server = new SimpleServer("TestInstance");
    Assert.IsNotNull(server);

    server.Start();
    
    Assert.IsTrue(server.IsRunning() == StatusCodes.BadServerHalted, $"Server status code {server.IsRunning()} was unexpected.");
    
    Console.WriteLine($"StartTime={server.GetStatus().StartTime} State={server.GetStatus().State}");
    var endpoints = server.GetEndpoints();
    Console.WriteLine($"{endpoints.Count} available Endpoints.");
    foreach (var endpoint in endpoints)
    {
      Console.WriteLine($"Available Endpoint: {endpoint.EndpointUrl} - {endpoint.SecurityPolicyUri}");
    }

    var client = new SimpleClient("TestInstance", DiscoveryUrl);
    var discoveredEndpoints = client.GetEndpoints();
    Console.WriteLine($"{discoveredEndpoints.Count} available Endpoints.");
    foreach (var discoveredEndpoint in discoveredEndpoints)
    {
      Console.WriteLine($"Available Endpoint: {discoveredEndpoint.EndpointUrl} - {discoveredEndpoint.SecurityPolicyUri}");
    }
    client.Connect();
    Assert.IsTrue(client.IsConnected(), "client.IsConnected()");
    var browsedNodeCount = client.Browse();
    Assert.IsTrue(browsedNodeCount > 0);
    Console.WriteLine($"Browsed {browsedNodeCount} nodes");
  }
}