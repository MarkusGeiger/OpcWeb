using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpcUaClient;

var sw = new Stopwatch();
sw.Restart();
//using var opcUaClient = new SimpleClient("ConsoleBrowser", "opc.tcp://localhost:50000");
using var opcUaClient = new SimpleClient("ConsoleBrowser", "opc.tcp://localhost:4840");
sw.Stop();
var configuredIn = sw.ElapsedMilliseconds;
Console.WriteLine($"Configuring finished in {configuredIn}ms");
sw.Restart();
opcUaClient.Connect();
sw.Stop();
var connectedIn = sw.ElapsedMilliseconds;
Console.WriteLine($"Connecting finished in {connectedIn}ms");
sw.Restart();
var browsedNodes = opcUaClient.Browse();
sw.Stop();
var browsedIn = sw.ElapsedMilliseconds;
sw.Restart();
var nodes = System.Text.Json.JsonSerializer.Serialize(opcUaClient.Nodes, new JsonSerializerOptions{WriteIndented = true});//,
//  new JsonSerializerOptions { TypeInfoResolver = Context.Default, WriteIndented = true });
sw.Stop();
var convertedIn = sw.ElapsedMilliseconds;
Console.WriteLine(nodes);
Console.WriteLine($"Browsing ‘{browsedNodes}‘ Nodes finished in {browsedIn}ms");
Console.WriteLine($"Configure: {configuredIn}ms; Connect: {connectedIn}ms; Browse: {browsedIn}ms; Convert: {convertedIn}ms");


// [JsonSourceGenerationOptions(WriteIndented = true)]
// [JsonSerializable(typeof(Node))]
// [JsonSerializable(typeof(List<Node>))]
// internal partial class Context : JsonSerializerContext
// {
// }