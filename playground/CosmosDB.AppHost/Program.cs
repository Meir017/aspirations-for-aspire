using Aspire.Hosting.ApplicationModel;
using Meir.Aspire.Azure.CosmosDB.ResourceClientReady;

var builder = DistributedApplication.CreateBuilder(args);

var cosmosdb = builder.AddAzureCosmosDB("cosmos-db")
    .RunAsEmulator();

cosmosdb.OnClientReady(async (CosmosDBClientReadyEvent evt, CancellationToken ct) =>
{
    var account = await evt.Client.ReadAccountAsync();
    Console.WriteLine($"CosmosDB Client Ready - Account: {account.Id}");
});

var database = cosmosdb.AddCosmosDatabase("testdb");

database.OnClientReady(async (CosmosDatabaseReadyEvent evt, CancellationToken ct) =>
{
    Console.WriteLine($"CosmosDB Database Ready - Database: {evt.Database.Id}");
});

var container = database.AddContainer("testcontainer", "/partitionKey");

container.OnClientReady(async (CosmosContainerReadyEvent evt, CancellationToken ct) =>
{
    Console.WriteLine($"CosmosDB Container Ready - Container: {evt.Container.Id}");
});

builder.Build().Run();
