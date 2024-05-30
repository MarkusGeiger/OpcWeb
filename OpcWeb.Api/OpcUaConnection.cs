using System.Text.Json.Serialization;
using OpcUaClient;

namespace OpcWeb.Api;

public class OpcUaConnection
{
  public int Id { get; set; }
  public bool Enabled { get; set; }
  public string DiscoveryUrl { get; set; }
  public bool Connected => Client?.IsConnected() ?? false;
  
  [JsonIgnore]
  public SimpleClient? Client { get; set; }

  public void Connect()
  {
    if (Enabled && !String.IsNullOrWhiteSpace(DiscoveryUrl) && DiscoveryUrl.StartsWith("opc.tcp://"))
    {
      if (Client == null)
      {
        Client = new SimpleClient("test", DiscoveryUrl);
      }

      Client.Connect();
    }
  }
}