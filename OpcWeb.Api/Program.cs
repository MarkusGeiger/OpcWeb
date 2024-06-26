using OpcUaClient;
using OpcWeb.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddSingleton<ConnectionHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/endpoints", (string url) =>
  {
    var client = new SimpleClient("OpcUaWebApi", url);
    return client.GetEndpoints().Select(EndpointDescriptionHelper.FormatToString);
  })
  .WithName("GetEndpoints")
  .WithOpenApi();


app.MapGet("/endpointdescriptions", (string url) =>
  {
    var client = new SimpleClient("OpcUaWebApi", url);
    return client.GetEndpoints();
  })
  .WithName("GetEndpointDescriptions")
  .WithOpenApi();

app.Run();
