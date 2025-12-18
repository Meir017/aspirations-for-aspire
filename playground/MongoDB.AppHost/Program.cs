using Aspire.Hosting.ApplicationModel;
using Meir.Aspire.MongoDB.ResourceClientReady;

var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("mongodb");

mongodb.OnClientReady(async (MongoClientReadyEvent evt, CancellationToken ct) =>
{
    var databases = await evt.Client.ListDatabaseNamesAsync(ct);
    Console.WriteLine($"MongoDB Client Ready - Databases available");
});

var database = mongodb.AddDatabase("testdb");

database.OnClientReady(async (MongoDatabaseReadyEvent evt, CancellationToken ct) =>
{
    Console.WriteLine($"MongoDB Database Ready - Database: {evt.Database.DatabaseNamespace.DatabaseName}");
});

builder.Build().Run();
