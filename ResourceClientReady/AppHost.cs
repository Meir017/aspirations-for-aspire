using Aspire.Hosting.Azure;
using Aspire.Hosting.Eventing;
using Azure.Storage.Blobs;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = DistributedApplication.CreateBuilder(args);

var cosmos = builder.AddAzureCosmosDB("cosmos-db")
    .RunAsEmulator(emulator => emulator.WithLifetime(ContainerLifetime.Persistent));

cosmos.OnClientReady(async (evt, ct) =>
{
    var client = evt.Client;
    Console.WriteLine($"CosmosDB Client is ready - account endpoint: {client.Endpoint}");
});

var cosmosDatabase = cosmos.AddCosmosDatabase("my-database");
cosmosDatabase.OnClientReady(async (evt, ct) =>
{
    var database = evt.Database;
    Console.WriteLine($"CosmosDB Database is ready - id: {database.Id}");
});

var cosmosContainer = cosmosDatabase.AddContainer("my-container", "/partitionKey");
cosmosContainer.OnClientReady(async (evt, ct) =>
{
    var container = evt.Container;
    Console.WriteLine($"CosmosDB Container is ready - id: {container.Id}");
});

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
    {
        if (!builder.Resource.TryGetLastAnnotation<BlobContainerClientReadyAnnotation>(out var existingEvent))
        {
            builder.WithAnnotation(new BlobContainerClientReadyAnnotation());
            builder.OnResourceReady(async (resource, evt, ct) =>
            {
                var containerClient = new BlobContainerClient(
                    await resource.ConnectionStringExpression.GetValueAsync(ct),
                    resource.Name);

                await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);

                var readyEvent = new BlobContainerClientReadyEvent(resource, containerClient);
                await builder.ApplicationBuilder.Eventing.PublishAsync(readyEvent, ct);
            });
        }
        return builder.OnEvent<AzureBlobStorageResource, BlobContainerClientReadyEvent>((resource, evt, ct) => handler(evt, ct));
    }

    private static IResourceBuilder<TResource> OnEvent<TResource, TEvent>(this IResourceBuilder<TResource> builder, Func<TResource, TEvent, CancellationToken, Task> callback) where TResource : IResource where TEvent : IDistributedApplicationResourceEvent
    {
        builder.ApplicationBuilder.Eventing.Subscribe(builder.Resource, (TEvent evt, CancellationToken ct) => callback(builder.Resource, evt, ct));
        return builder;
    }

    private class BlobContainerClientReadyAnnotation : IResourceAnnotation;
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
        if (!builder.Resource.TryGetLastAnnotation<MongoDatabaseReadyAnnotation>(out var existingEvent))
        {
            builder.WithAnnotation(new MongoDatabaseReadyAnnotation());
            builder.OnResourceReady(async (resource, evt, ct) =>
            {
                var parentConnectionString = await resource.Parent.ConnectionStringExpression.GetValueAsync(ct);
                IMongoClient client = new MongoClient(parentConnectionString);
                IMongoDatabase database = client.GetDatabase(resource.Name);

                var readyEvent = new MongoDatabaseReadyEvent(resource.Parent, database);
                await builder.ApplicationBuilder.Eventing.PublishAsync(readyEvent, ct);
            });
        }
        return builder.OnEvent((MongoDBDatabaseResource resource, MongoDatabaseReadyEvent evt, CancellationToken ct) => handler(evt, ct));
    }

    private class MongoDatabaseReadyAnnotation : IResourceAnnotation
    {
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

public static class AzureCosmosExtensions
{
    public static IResourceBuilder<AzureCosmosDBResource> OnClientReady(this IResourceBuilder<AzureCosmosDBResource> builder, Func<CosmosDBClientReadyEvent, CancellationToken, Task> handler)
    {
        if (!builder.Resource.TryGetLastAnnotation<CosmosDBClientReadyAnnotation>(out var existingEvent))
        {
            builder.WithAnnotation(new CosmosDBClientReadyAnnotation());
            builder.OnResourceReady(async (resource, evt, ct) =>
            {
                var connectionString = await resource.ConnectionStringExpression.GetValueAsync(ct);
                var client = new Microsoft.Azure.Cosmos.CosmosClient(connectionString);

                var readyEvent = new CosmosDBClientReadyEvent(resource, client);
                await builder.ApplicationBuilder.Eventing.PublishAsync(readyEvent, ct);
            });
        }
        return builder.OnEvent((AzureCosmosDBResource resource, CosmosDBClientReadyEvent evt, CancellationToken ct) => handler(evt, ct));
    }

    // for database
    public static IResourceBuilder<AzureCosmosDBDatabaseResource> OnClientReady(this IResourceBuilder<AzureCosmosDBDatabaseResource> builder, Func<CosmosDatabaseReadyEvent, CancellationToken, Task> handler)
    {
        if (!builder.Resource.TryGetLastAnnotation<CosmosDBClientReadyAnnotation>(out var existingEvent))
        {
            builder.WithAnnotation(new CosmosDBClientReadyAnnotation());
            builder.OnResourceReady(async (resource, evt, ct) =>
            {
                var connectionString = await resource.Parent.ConnectionStringExpression.GetValueAsync(ct);
                var cosmosClient = new Microsoft.Azure.Cosmos.CosmosClient(connectionString);
                var database = cosmosClient.GetDatabase(resource.Name);

                var readyEvent = new CosmosDatabaseReadyEvent(resource.Parent, database);
                await builder.ApplicationBuilder.Eventing.PublishAsync(readyEvent, ct);
            });
        }
        return builder.OnEvent((AzureCosmosDBDatabaseResource resource, CosmosDatabaseReadyEvent evt, CancellationToken ct) => handler(evt, ct));
    }

    // for container
    public static IResourceBuilder<AzureCosmosDBContainerResource> OnClientReady(this IResourceBuilder<AzureCosmosDBContainerResource> builder, Func<CosmosContainerReadyEvent, CancellationToken, Task> handler)
    {
        if (!builder.Resource.TryGetLastAnnotation<CosmosDBClientReadyAnnotation>(out var existingEvent))
        {
            builder.WithAnnotation(new CosmosDBClientReadyAnnotation());
            builder.OnResourceReady(async (resource, evt, ct) =>
            {
                var connectionString = await resource.Parent.Parent.ConnectionStringExpression.GetValueAsync(ct);
                var cosmosClient = new Microsoft.Azure.Cosmos.CosmosClient(connectionString);
                var database = cosmosClient.GetDatabase(resource.Parent.Name);
                var container = database.GetContainer(resource.Name);

                var readyEvent = new CosmosContainerReadyEvent(resource, container);
                await builder.ApplicationBuilder.Eventing.PublishAsync(readyEvent, ct);
            });
        }
        return builder.OnEvent((AzureCosmosDBContainerResource resource, CosmosContainerReadyEvent evt, CancellationToken ct) => handler(evt, ct));
    }

    private class CosmosDBClientReadyAnnotation : IResourceAnnotation
    {
    }

    private static IResourceBuilder<TResource> OnEvent<TResource, TEvent>(this IResourceBuilder<TResource> builder, Func<TResource, TEvent, CancellationToken, Task> callback) where TResource : IResource where TEvent : IDistributedApplicationResourceEvent
    {
        builder.ApplicationBuilder.Eventing.Subscribe(builder.Resource, (TEvent evt, CancellationToken ct) => callback(builder.Resource, evt, ct));
        return builder;
    }
}

public record class CosmosDBClientReadyEvent(AzureCosmosDBResource Resource, Microsoft.Azure.Cosmos.CosmosClient Client) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}

public record class CosmosDatabaseReadyEvent(AzureCosmosDBResource Resource, Microsoft.Azure.Cosmos.Database Database) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}

public record class CosmosContainerReadyEvent(AzureCosmosDBContainerResource Resource, Microsoft.Azure.Cosmos.Container Container) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}