using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Eventing;
using MongoDB.Driver;

namespace Meir.Aspire.MongoDB.ResourceClientReady;

/// <summary>
/// Provides extension methods for MongoDB resource builders to subscribe to client ready events.
/// </summary>
public static class MongoDBHostingExtensions
{
    /// <summary>
    /// Registers a callback that will be invoked when the MongoDB client is initialized and ready.
    /// </summary>
    /// <param name="builder">The resource builder for the MongoDB server resource.</param>
    /// <param name="handler">The callback to invoke when the client is ready.</param>
    /// <returns>The resource builder for chaining.</returns>
    /// <remarks>
    /// This extension method subscribes to the resource ready event and creates a MongoClient
    /// using the connection string from the resource. The handler is invoked with the initialized client.
    /// </remarks>
    public static IResourceBuilder<MongoDBServerResource> OnClientReady(
        this IResourceBuilder<MongoDBServerResource> builder, 
        Func<MongoClientReadyEvent, CancellationToken, Task> handler)
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

    /// <summary>
    /// Registers a callback that will be invoked when the MongoDB database is initialized and ready.
    /// </summary>
    /// <param name="builder">The resource builder for the MongoDB database resource.</param>
    /// <param name="handler">The callback to invoke when the database is ready.</param>
    /// <returns>The resource builder for chaining.</returns>
    /// <remarks>
    /// This extension method subscribes to the resource ready event and creates a Database instance
    /// from the parent MongoClient. The handler is invoked with the initialized database.
    /// </remarks>
    public static IResourceBuilder<MongoDBDatabaseResource> OnClientReady(
        this IResourceBuilder<MongoDBDatabaseResource> builder, 
        Func<MongoDatabaseReadyEvent, CancellationToken, Task> handler)
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

    private static IResourceBuilder<TResource> OnEvent<TResource, TEvent>(
        this IResourceBuilder<TResource> builder, 
        Func<TResource, TEvent, CancellationToken, Task> callback) 
        where TResource : IResource 
        where TEvent : IDistributedApplicationResourceEvent
    {
        builder.ApplicationBuilder.Eventing.Subscribe(builder.Resource, (TEvent evt, CancellationToken ct) => callback(builder.Resource, evt, ct));
        return builder;
    }

    private class MongoClientReadyAnnotation : IResourceAnnotation
    {
    }

    private class MongoDatabaseReadyAnnotation : IResourceAnnotation
    {
    }
}
