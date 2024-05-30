using System.Net;
using Opc.Ua;
using Opc.Ua.Configuration;
using Opc.Ua.Server;

namespace OpcUaServer;

public class SimpleServer : StandardServer
{
  private readonly ApplicationConfiguration _config;
  private readonly ApplicationInstance _application;

  public SimpleServer(string applicationName)
  {
    Console.WriteLine("Configuring UA Server");
    // var certificateStorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\";
    var certificateStorePath = @"./OPC_Foundation/CertificateStores/";
    _config = new ApplicationConfiguration
    {
      ApplicationName = applicationName,
      ApplicationUri = Utils.Format(@"urn:{0}:" + applicationName, Dns.GetHostName()),
      ApplicationType = ApplicationType.Server,
      SecurityConfiguration = new SecurityConfiguration
      {
        ApplicationCertificate = new CertificateIdentifier
        {
          StoreType = @"Directory", StorePath = Path.Combine(certificateStorePath, "MachineDefault"),
          SubjectName = applicationName + "Server"
        },
        TrustedIssuerCertificates = new CertificateTrustList
          { StoreType = @"Directory", StorePath = Path.Combine(certificateStorePath, "UA Certificate Authorities") },
        TrustedPeerCertificates = new CertificateTrustList
          { StoreType = @"Directory", StorePath = Path.Combine(certificateStorePath, "UA Applications") },
        RejectedCertificateStore = new CertificateTrustList
          { StoreType = @"Directory", StorePath = Path.Combine(certificateStorePath, "RejectedCertificates") },
        AutoAcceptUntrustedCertificates = true
      },
      TransportConfigurations = new TransportConfigurationCollection(),
      TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
      ServerConfiguration = new ServerConfiguration{BaseAddresses = ["opc.tcp://localhost:4841"]},
      // ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
      TraceConfiguration = new TraceConfiguration()
    };
    _config.Validate(ApplicationType.Server).GetAwaiter().GetResult();
    if (_config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
      _config.CertificateValidator.CertificateValidation += (_, e) =>
      {
        e.Accept = e.Error.StatusCode == StatusCodes.BadCertificateUntrusted;
      };

    _application = new ApplicationInstance
    {
      ApplicationName = applicationName + "Server",
      ApplicationType = ApplicationType.Server,
      ApplicationConfiguration = _config
    };

    _application.CheckApplicationInstanceCertificate(false, 2048).GetAwaiter().GetResult();
  }

  public void Start()
  {
    Console.WriteLine("Starting OPC-UA Server.");
    base.Start(_config);
  }
  
  public StatusCode IsRunning()
  {
    if(_application.Server == null) return StatusCodes.BadServerHalted;
    
    Console.WriteLine($"{_application.Server.ServerError}");
    return _application.Server?.ServerError?.StatusCode ?? StatusCodes.Good;
  }
  //
  // #region Overridden Methods
  //
  // /// <summary>
  // ///   Creates the node managers for the server.
  // /// </summary>
  // /// <remarks>
  // ///   This method allows the sub-class create any additional node managers which it uses. The SDK
  // ///   always creates a CoreNodeManager which handles the built-in nodes defined by the specification.
  // ///   Any additional NodeManagers are expected to handle application specific nodes.
  // /// </remarks>
  protected override MasterNodeManager CreateMasterNodeManager(IServerInternal server,
    ApplicationConfiguration configuration)
  {
    Utils.Trace("Creating the Node Managers.");
  
    var nodeManagers = new List<INodeManager>();
  
    // create the custom node managers.
    //nodeManagers.Add(new SimpleNodeManager(server, configuration));
  
    // create master node manager.
    return new MasterNodeManager(server, configuration, null, nodeManagers.ToArray());
  }
  //
  // /// <summary>
  // ///   Loads the non-configurable properties for the application.
  // /// </summary>
  // /// <remarks>
  // ///   These properties are exposed by the server but cannot be changed by administrators.
  // /// </remarks>
  // protected override ServerProperties LoadServerProperties()
  // {
  //   var properties = new ServerProperties();
  //
  //   properties.ManufacturerName = "OPC Foundation";
  //   properties.ProductName = "Quickstart Empty Server";
  //   properties.ProductUri = "http://opcfoundation.org/Quickstart/EmptyServer/v1.0";
  //   properties.SoftwareVersion = Utils.GetAssemblySoftwareVersion();
  //   properties.BuildNumber = Utils.GetAssemblyBuildNumber();
  //   properties.BuildDate = Utils.GetAssemblyTimestamp();
  //
  //   // TBD - All applications have software certificates that need to added to the properties.
  //
  //   return properties;
  // }
  //
  // #endregion
}