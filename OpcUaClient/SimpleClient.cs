using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;

namespace OpcUaClient;

/// <summary>
/// Simple Client implementation using UA SDK .NET Standard from OPC Foundation
/// Implementation adapted from: https://youtu.be/gxA7SDNLHgc?si=_WDEntI8PsPmIIcl
/// </summary>
public class SimpleClient : IDisposable
{
  private readonly string _discoveryUrl;
  private readonly ApplicationConfiguration _config;
  private EndpointDescription? _selectedEndpoint;
  private Session? _session;
  private long _browsedNodes;
  public List<Node> Nodes { get; set; } = [];

  public SimpleClient(string applicationName, string discoveryUrl)
  {
    _discoveryUrl = discoveryUrl;
    
    Console.WriteLine("Configuring UA Client");
    // var certificateStorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\";
    var certificateStorePath = @"./OPC_Foundation/CertificateStores/";
    _config = new ApplicationConfiguration()
    {
      ApplicationName = applicationName,
      ApplicationUri = Utils.Format(@"urn:{0}:" + applicationName, System.Net.Dns.GetHostName()),
      ApplicationType = ApplicationType.Client,
      SecurityConfiguration = new SecurityConfiguration
      {
        ApplicationCertificate = new CertificateIdentifier { StoreType = @"Directory", StorePath = Path.Combine(certificateStorePath, "MachineDefault"), SubjectName = applicationName + "Client" },
        TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = Path.Combine(certificateStorePath, "UA Certificate Authorities") },
        TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = Path.Combine(certificateStorePath, "UA Applications") },
        RejectedCertificateStore = new CertificateTrustList { StoreType = @"Directory", StorePath = Path.Combine(certificateStorePath, "RejectedCertificates") },
        AutoAcceptUntrustedCertificates = true
      },
      TransportConfigurations = new TransportConfigurationCollection(),
      TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
      ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
      TraceConfiguration = new TraceConfiguration()
    };
    _config.Validate(ApplicationType.Client).GetAwaiter().GetResult();
    if (_config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
    {
      _config.CertificateValidator.CertificateValidation += (_, e) => { e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted); };
    }


    var application = new ApplicationInstance
    {
      ApplicationName = applicationName + "Client",
      ApplicationType = ApplicationType.Client,
      ApplicationConfiguration = _config
    };

    application.CheckApplicationInstanceCertificate(false, 2048).GetAwaiter().GetResult();
  }

  public void Connect()
  {
    try
    {
      var discoverTimeout = CoreClientUtils.DefaultDiscoverTimeout;
      var useSecurity = false;
      
      //var identity = new UserIdentity("username", "password");
      IUserIdentity? identity = null;
      // _selectedEndpoint = CoreClientUtils.SelectEndpoint(_config, _discoveryUrl, useSecurity: false);
      // _selectedEndpoint = CoreClientUtils.SelectEndpoint(_discoveryUrl, useSecurity: false);
      EndpointDescription _selectedEndpoint;
      
      
      var uri = CoreClientUtils.GetDiscoveryUrl(_discoveryUrl);
      var endpointConfiguration = EndpointConfiguration.Create();
      endpointConfiguration.OperationTimeout = discoverTimeout;

      using (var client = DiscoveryClient.Create(_config, uri, endpointConfiguration))
      {
        // Connect to the server's discovery endpoint and find the available configuration.
        Uri url = new Uri(client.Endpoint.EndpointUrl);
        var endpoints = client.GetEndpoints(null);
        foreach (var endpoint in endpoints) 
        {
          Console.WriteLine($"Available Endpoint:  {Utils.Format(
            "{0} - [{1}:{2}]",
            endpoint.EndpointUrl,
            endpoint.SecurityMode,
            SecurityPolicies.GetDisplayName(endpoint.SecurityPolicyUri))}");
        }
        var selectedEndpoint = CoreClientUtils.SelectEndpoint(url, endpoints, useSecurity);

        Uri endpointUrl = Utils.ParseUri(selectedEndpoint.EndpointUrl);
        if (endpointUrl != null && endpointUrl.Scheme == uri.Scheme)
        {
          UriBuilder builder = new UriBuilder(endpointUrl);
          builder.Host = uri.DnsSafeHost;
          builder.Port = uri.Port;
          selectedEndpoint.EndpointUrl = builder.ToString();
        }

        _selectedEndpoint = selectedEndpoint;
      }
      
      
      
      Console.WriteLine($"Selected Endpoint:   {_selectedEndpoint.EndpointUrl} - [{_selectedEndpoint.SecurityMode}:{SecurityPolicies.GetDisplayName(_selectedEndpoint.SecurityPolicyUri)}]");
      var configuredEndpoint = new ConfiguredEndpoint(null, _selectedEndpoint, EndpointConfiguration.Create(_config));
      Console.WriteLine("======================");
      Console.WriteLine($"Configured Endpoint: {configuredEndpoint}");
      var sessionTask = Session.Create(_config, configuredEndpoint, false, "TestSession", 60000, identity, null);
      _session = sessionTask.GetAwaiter().GetResult();
    }
    catch (Exception e)
    {
      Console.WriteLine($"Failed to initialize session: {e}");
    }
  }

  public long Browse()
  {
    if (_session is not { Connected: true })
    {
      Console.WriteLine("Session is not connected!");
      return 0;
    }

    _browsedNodes = 0;
    Console.WriteLine("Step 3 - Browse the server namespace.");
    try
    {
      Console.WriteLine("DisplayName: BrowseName, NodeClass, NodeId");
      var nodes = new List<Node>();
      BrowseAllChildren(ObjectIds.ObjectsFolder, ref nodes);
      Nodes = nodes;
    }
    catch (Exception e)
    {
      Console.WriteLine($"Failed to browse Server: {e}");
    }

    return _browsedNodes;
  }

  private void BrowseAllChildren(NodeId nodeId, ref List<Node> children, int level = 0)
  {
    if (_session is not { Connected: true })
    {
      Console.WriteLine("Session is not connected!");
      return;
    }

    var currentLevel = level + 1;
    var refs = BrowseReferenceDescriptions(nodeId);
    if (refs == null) return;
    foreach (var rd in refs)
    {
      _browsedNodes++;
      
      Console.WriteLine($"{(level > 0 ? new string('+', level) : "")} {rd.DisplayName}: {rd.BrowseName}, {rd.NodeClass}, {rd.NodeId}");
      var currentNode = new Node
      {
        DisplayName = rd.DisplayName.ToString(),
        BrowseName = rd.BrowseName.ToString(),
        NodeClass = rd.NodeClass.ToString(),
        NodeId = ExpandedNodeId.ToNodeId(rd.NodeId, _session.NamespaceUris).ToString()
      };
      children.Add(currentNode);
      var currentChildren = new List<Node>();
      BrowseAllChildren(ExpandedNodeId.ToNodeId(rd.NodeId, _session.NamespaceUris), ref currentChildren, currentLevel);
      currentNode.Children = currentChildren;
    }
  }
  
  private ReferenceDescriptionCollection? BrowseReferenceDescriptions(NodeId nodeId)
  {
    if (_session is not { Connected: true })
    {
      Console.WriteLine("Session is not connected!");
      return null;
    }

    try
    {
      _session.Browse(null, null, nodeId, 0u, BrowseDirection.Forward, ReferenceTypeIds.HierarchicalReferences, true, (uint)NodeClass.Variable | (uint)NodeClass.Object | (uint)NodeClass.Method, out _, out var referenceDescriptions);
      return referenceDescriptions;
    }
    catch (ServiceResultException e)
    {
      Console.WriteLine(e);
      return null;
    }
  }

  public void Dispose()
  {
    GC.SuppressFinalize(this);
    _session?.Dispose();
  }
}