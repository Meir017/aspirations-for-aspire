using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Eventing;
using Azure.Storage.Blobs;

namespace Meir.Aspire.Azure.Storage.ResourceClientReady;

/// <summary>
/// Provides extension methods for Azure Storage resource builders to subscribe to client ready events.
/// </summary>
public static class AzureStorageExtensions
{
    /// <summary>
    /// Registers a callback that will be invoked when the Azure Blob Container client is initialized and ready.
    /// </summary>
    /// <param name="builder">The resource builder for the Azure Blob Storage resource.</param>
    /// <param name="handler">The callback to invoke when the client is ready.</param>
    /// <returns>The resource builder for chaining.</returns>
    /// <remarks>
    /// This extension method subscribes to the resource ready event and creates a BlobContainerClient
    /// using the connection string from the resource. The container is created if it doesn't exist.
    /// The handler is invoked with the initialized client.
    /// </remarks>
    public static IResourceBuilder<AzureBlobStorageResource> OnClientReady(
        this IResourceBuilder<AzureBlobStorageResource> builder,
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

    private static IResourceBuilder<TResource> OnEvent<TResource, TEvent>(
        this IResourceBuilder<TResource> builder, 
        Func<TResource, TEvent, CancellationToken, Task> callback) 
        where TResource : IResource 
        where TEvent : IDistributedApplicationResourceEvent
    {
        builder.ApplicationBuilder.Eventing.Subscribe(builder.Resource, (TEvent evt, CancellationToken ct) => callback(builder.Resource, evt, ct));
        return builder;
    }

    private class BlobContainerClientReadyAnnotation : IResourceAnnotation
    {
    }
}
