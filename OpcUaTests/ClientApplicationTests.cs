using OpcUaClient;

namespace OpcUaTests;

[TestClass]
public class ClientApplicationTests
{
  private const string DiscoveryUrl = "opc.tcp://localhost:62541";
  
  [TestMethod]
  public void CreateInstanceSuccess()
  {
    var client = new SimpleClient("TestInstance", DiscoveryUrl);
    Assert.IsNotNull(client, "Client could not be created.");
  }

  [TestMethod]
  public void DiscoverEndpointsSuccess()
  {
    var client = new SimpleClient("TestInstance", DiscoveryUrl);
    Assert.IsNotNull(client, "Client could not be created.");
    var endpoints = client.GetEndpoints();
    Assert.IsNotNull(endpoints, "endpoints != null");
    Assert.IsTrue(endpoints.Count > 0, "endpoints.Count > 0");
    foreach (var endpoint in endpoints)
    {
      Console.WriteLine($"{endpoint.EndpointUrl} - {endpoint.SecurityPolicyUri} - {endpoint.SecurityLevel}");
    }
  }

  [TestMethod]
  public void ConnectServerSuccess()
  {
    var client = new SimpleClient("TestInstance", DiscoveryUrl);
    Assert.IsNotNull(client, "Client could not be created.");
    var endpoints = client.GetEndpoints();
    Assert.IsNotNull(endpoints, "endpoints != null");
    Assert.IsTrue(endpoints.Count > 0, "endpoints.Count > 0");
    client.Connect();
    
    Assert.IsTrue(client.IsConnected(), "client.IsConnected()");
  }
}