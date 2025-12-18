using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Eventing;
using Microsoft.Azure.Cosmos;

namespace Meir.Aspire.Azure.CosmosDB.ResourceClientReady;

/// <summary>
/// Provides extension methods for Azure Cosmos DB resource builders to subscribe to client ready events.
/// </summary>
public static class AzureCosmosExtensions
{
    /// <summary>
    /// Registers a callback that will be invoked when the Cosmos DB client is initialized and ready.
    /// </summary>
    /// <param name="builder">The resource builder for the Azure Cosmos DB resource.</param>
    /// <param name="handler">The callback to invoke when the client is ready.</param>
    /// <returns>The resource builder for chaining.</returns>
    /// <remarks>
    /// This extension method subscribes to the resource ready event and creates a CosmosClient
    /// using the connection string from the resource. The handler is invoked with the initialized client.
    /// </remarks>
    public static IResourceBuilder<AzureCosmosDBResource> OnClientReady(
        this IResourceBuilder<AzureCosmosDBResource> builder, 
        Func<CosmosDBClientReadyEvent, CancellationToken, Task> handler)
    {
        if (!builder.Resource.TryGetLastAnnotation<CosmosDBClientReadyAnnotation>(out var existingEvent))
        {
            builder.WithAnnotation(new CosmosDBClientReadyAnnotation());
            builder.OnResourceReady(async (resource, evt, ct) =>
            {
                var connectionString = await resource.ConnectionStringExpression.GetValueAsync(ct);
                var client = new CosmosClient(connectionString);

                var readyEvent = new CosmosDBClientReadyEvent(resource, client);
                await builder.ApplicationBuilder.Eventing.PublishAsync(readyEvent, ct);
            });
        }
        return builder.OnEvent((AzureCosmosDBResource resource, CosmosDBClientReadyEvent evt, CancellationToken ct) => handler(evt, ct));
    }

    /// <summary>
    /// Registers a callback that will be invoked when the Cosmos DB database is initialized and ready.
    /// </summary>
    /// <param name="builder">The resource builder for the Azure Cosmos DB database resource.</param>
    /// <param name="handler">The callback to invoke when the database is ready.</param>
    /// <returns>The resource builder for chaining.</returns>
    /// <remarks>
    /// This extension method subscribes to the resource ready event and creates a Database instance
    /// from the parent CosmosClient. The handler is invoked with the initialized database.
    /// </remarks>
    public static IResourceBuilder<AzureCosmosDBDatabaseResource> OnClientReady(
        this IResourceBuilder<AzureCosmosDBDatabaseResource> builder, 
        Func<CosmosDatabaseReadyEvent, CancellationToken, Task> handler)
    {
        if (!builder.Resource.TryGetLastAnnotation<CosmosDBClientReadyAnnotation>(out var existingEvent))
        {
            builder.WithAnnotation(new CosmosDBClientReadyAnnotation());
            builder.OnResourceReady(async (resource, evt, ct) =>
            {
                var connectionString = await resource.Parent.ConnectionStringExpression.GetValueAsync(ct);
                var cosmosClient = new CosmosClient(connectionString);
                var database = cosmosClient.GetDatabase(resource.Name);

                var readyEvent = new CosmosDatabaseReadyEvent(resource.Parent, database);
                await builder.ApplicationBuilder.Eventing.PublishAsync(readyEvent, ct);
            });
        }
        return builder.OnEvent((AzureCosmosDBDatabaseResource resource, CosmosDatabaseReadyEvent evt, CancellationToken ct) => handler(evt, ct));
    }

    /// <summary>
    /// Registers a callback that will be invoked when the Cosmos DB container is initialized and ready.
    /// </summary>
    /// <param name="builder">The resource builder for the Azure Cosmos DB container resource.</param>
    /// <param name="handler">The callback to invoke when the container is ready.</param>
    /// <returns>The resource builder for chaining.</returns>
    /// <remarks>
    /// This extension method subscribes to the resource ready event and creates a Container instance
    /// from the parent Database. The handler is invoked with the initialized container.
    /// </remarks>
    public static IResourceBuilder<AzureCosmosDBContainerResource> OnClientReady(
        this IResourceBuilder<AzureCosmosDBContainerResource> builder, 
        Func<CosmosContainerReadyEvent, CancellationToken, Task> handler)
    {
        if (!builder.Resource.TryGetLastAnnotation<CosmosDBClientReadyAnnotation>(out var existingEvent))
        {
            builder.WithAnnotation(new CosmosDBClientReadyAnnotation());
            builder.OnResourceReady(async (resource, evt, ct) =>
            {
                var connectionString = await resource.Parent.Parent.ConnectionStringExpression.GetValueAsync(ct);
                var cosmosClient = new CosmosClient(connectionString);
                var database = cosmosClient.GetDatabase(resource.Parent.Name);
                var container = database.GetContainer(resource.Name);

                var readyEvent = new CosmosContainerReadyEvent(resource, container);
                await builder.ApplicationBuilder.Eventing.PublishAsync(readyEvent, ct);
            });
        }
        return builder.OnEvent((AzureCosmosDBContainerResource resource, CosmosContainerReadyEvent evt, CancellationToken ct) => handler(evt, ct));
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

    private class CosmosDBClientReadyAnnotation : IResourceAnnotation
    {
    }
}
