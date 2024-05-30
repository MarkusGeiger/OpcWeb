using Opc.Ua;
using Opc.Ua.Server;

namespace OpcUaServer;

public class SimpleNodeManager : CustomNodeManager2
{
  public SimpleNodeManager(IServerInternal server, ApplicationConfiguration configuration) : base(server, configuration)
  {
    SystemContext.NodeIdFactory = this;
    
    string[] namespaceUrls = new string[1];
    namespaceUrls[0] = Namespaces.Simple;
    SetNamespaces(namespaceUrls);
  }
  
  protected SimpleNodeManager(IServerInternal server, params string[] namespaceUris) : base(server, namespaceUris)
  {
  }

  protected SimpleNodeManager(IServerInternal server, ApplicationConfiguration configuration, params string[] namespaceUris) : base(server, configuration, namespaceUris)
  {
  }
}