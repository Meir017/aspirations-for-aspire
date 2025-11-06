using Aspire.Hosting.Azure;
using Aspire.Hosting.Eventing;
using Azure.Storage.Blobs;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = DistributedApplication.CreateBuilder(args);

var cosmos = builder.AddAzureCosmosDB("cosmos-db")
    .RunAsEmulator(emulator => emulator.WithLifetime(ContainerLifetime.Persistent));

var cosmosDatabase = cosmos.AddCosmosDatabase("my-database");

var cosmosContainer = cosmosDatabase.AddContainer("my-container", "/partitionKey");

var mongo = builder.AddMongoDB("mongo-db");
mongo.OnClientReady(async (evt, ct) =>
{
    var client = evt.Client;
    Console.WriteLine($"MongoDB Client is ready - cluster: {client.Cluster.Description.ClusterId}");    
});

var mongoDatabase = mongo.AddDatabase("my-mongo-database");
mongoDatabase.OnClientReady(async (evt, ct) =>
{
    var database = evt.Database;
    Console.WriteLine($"MongoDB Database is ready - name: {database.DatabaseNamespace.DatabaseName}");
    var collection = database.GetCollection<BsonDocument>("sample-collection");
    await collection.InsertOneAsync(new BsonDocument { { "message", "Hello, MongoDB!" } }, default, ct);
});

var storage = builder.AddAzureStorage("azure-storage")
    .RunAsEmulator(emulator => emulator.WithLifetime(ContainerLifetime.Persistent));

var blobStorage = storage.AddBlobs("my-blob-container");
blobStorage.OnClientReady(async (evt, ct) =>
{
    var client = evt.Client;
    await client.UploadBlobAsync("sample.txt", BinaryData.FromString("Hello, World!"), cancellationToken: ct);
    Console.WriteLine("Uploaded sample.txt to blob storage.");
});

builder.Build().Run();


public static class AzureStorageExtensions
{
    public static IResourceBuilder<AzureBlobStorageResource> OnClientReady(this IResourceBuilder<AzureBlobStorageResource> builder,
        Func<BlobContainerClientReadyEvent, CancellationToken, Task> handler)
        => builder.OnEvent<AzureBlobStorageResource, BlobContainerClientReadyEvent>((resource, evt, ct) => handler(evt, ct));

    private static IResourceBuilder<TResource> OnEvent<TResource, TEvent>(this IResourceBuilder<TResource> builder, Func<TResource, TEvent, CancellationToken, Task> callback) where TResource : IResource where TEvent : IDistributedApplicationResourceEvent
    {
        builder.ApplicationBuilder.Eventing.Subscribe(builder.Resource, (TEvent evt, CancellationToken ct) => callback(builder.Resource, evt, ct));
        return builder;
    }
}

public record class BlobContainerClientReadyEvent(AzureBlobStorageResource Resource, BlobContainerClient Client) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}

public static class MongoDBHostingExtensions
{
    public static IResourceBuilder<MongoDBServerResource> OnClientReady(this IResourceBuilder<MongoDBServerResource> builder, Func<MongoClientReadyEvent, CancellationToken, Task> handler)
    {
        if (!builder.Resource.TryGetLastAnnotation<MongoClientReadyAnnotation>(out var existingEvent))
        {
            builder.WithAnnotation(new MongoClientReadyAnnotation());
            builder.OnResourceReady(async (resource, evt, ct) =>
            {
                var connectionString = await resource.ConnectionStringExpression.GetValueAsync(ct);
                IMongoClient client = new MongoClient(connectionString);
                
                var readyEvent = new MongoClientReadyEvent(resource, client);
                await builder.ApplicationBuilder.Eventing.PublishAsync(readyEvent, ct);
            });
        }

        return builder.OnEvent((MongoDBServerResource resource, MongoClientReadyEvent evt, CancellationToken ct) => handler(evt, ct));
    }

    private class MongoClientReadyAnnotation : IResourceAnnotation
    {
    }

    public static IResourceBuilder<MongoDBDatabaseResource> OnClientReady(this IResourceBuilder<MongoDBDatabaseResource> builder, Func<MongoDatabaseReadyEvent, CancellationToken, Task> handler)
    {
        return builder.OnEvent((MongoDBDatabaseResource resource, MongoDatabaseReadyEvent evt, CancellationToken ct) => handler(evt, ct));
    }

    private static IResourceBuilder<TResource> OnEvent<TResource, TEvent>(this IResourceBuilder<TResource> builder, Func<TResource, TEvent, CancellationToken, Task> callback) where TResource : IResource where TEvent : IDistributedApplicationResourceEvent
    {
        builder.ApplicationBuilder.Eventing.Subscribe(builder.Resource, (TEvent evt, CancellationToken ct) => callback(builder.Resource, evt, ct));
        return builder;
    }
}

public record class MongoClientReadyEvent(MongoDBServerResource Resource, IMongoClient Client) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}

public record class MongoDatabaseReadyEvent(MongoDBServerResource Resource, IMongoDatabase Database) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}