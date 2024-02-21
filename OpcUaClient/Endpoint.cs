using Opc.Ua;

namespace OpcUaClient;

public static class EndpointDescriptionHelper
{
  public static string FormatToString(EndpointDescription endpointDescription)
  {
    return $"{endpointDescription.EndpointUrl} - " +
           $"[{endpointDescription.SecurityMode}:{SecurityPolicies.GetDisplayName(endpointDescription.SecurityPolicyUri)}]" +
           $" {endpointDescription.Server.ProductUri}";
  }
}