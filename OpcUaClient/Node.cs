namespace OpcUaClient;

public class Node
{
  public string DisplayName { get; set; }
  public string BrowseName { get; set; }
  public string NodeClass { get; set; }
  public string NodeId { get; set; }
  public List<Node> Children { get; set; } = [];
  public override string ToString()
  {
    return $"{DisplayName} ({NodeClass})";
  }
}