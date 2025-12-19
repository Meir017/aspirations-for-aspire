using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Eventing;
using Azure.Messaging.ServiceBus;

namespace Meir.Aspire.Azure.ServiceBus.ResourceClientReady;

/// <summary>
/// Provides extension methods for Azure Service Bus resource builders to subscribe to client ready events.
/// </summary>
public static class AzureServiceBusExtensions
{
    /// <summary>
    /// Registers a callback that will be invoked when the Service Bus client is initialized and ready.
    /// </summary>
    /// <param name="builder">The resource builder for the Azure Service Bus resource.</param>
    /// <param name="handler">The callback to invoke when the client is ready.</param>
    /// <returns>The resource builder for chaining.</returns>
    /// <remarks>
    /// This extension method subscribes to the resource ready event and creates a ServiceBusClient
    /// using the connection string from the resource. The handler is invoked with the initialized client.
    /// </remarks>
    public static IResourceBuilder<AzureServiceBusResource> OnClientReady(
        this IResourceBuilder<AzureServiceBusResource> builder,
        Func<Events.ServiceBusClientReadyEvent, CancellationToken, Task> handler)
    {
        if (!builder.Resource.TryGetLastAnnotation<ServiceBusClientReadyAnnotation>(out var existingEvent))
        {
            builder.WithAnnotation(new ServiceBusClientReadyAnnotation());
            builder.OnResourceReady(async (resource, evt, ct) =>
            {
                var connectionString = await resource.ConnectionStringExpression.GetValueAsync(ct);
                var client = new ServiceBusClient(connectionString);

                var readyEvent = new Events.ServiceBusClientReadyEvent(resource, client);
                await builder.ApplicationBuilder.Eventing.PublishAsync(readyEvent, ct);
            });
        }
        return builder.OnEvent((AzureServiceBusResource resource, Events.ServiceBusClientReadyEvent evt, CancellationToken ct) => handler(evt, ct));
    }

    /// <summary>
    /// Registers a callback that will be invoked when the Service Bus queue sender is initialized and ready.
    /// </summary>
    /// <param name="builder">The resource builder for the Azure Service Bus queue resource.</param>
    /// <param name="handler">The callback to invoke when the queue sender is ready.</param>
    /// <returns>The resource builder for chaining.</returns>
    /// <remarks>
    /// This extension method subscribes to the resource ready event and creates a ServiceBusSender
    /// from the parent ServiceBusClient. The handler is invoked with the initialized sender.
    /// </remarks>
    public static IResourceBuilder<AzureServiceBusQueueResource> OnClientReady(
        this IResourceBuilder<AzureServiceBusQueueResource> builder,
        Func<Events.ServiceBusQueueReadyEvent, CancellationToken, Task> handler)
    {
        if (!builder.Resource.TryGetLastAnnotation<ServiceBusQueueReadyAnnotation>(out var existingEvent))
        {
            builder.WithAnnotation(new ServiceBusQueueReadyAnnotation());
            builder.OnResourceReady(async (resource, evt, ct) =>
            {
                var connectionString = await resource.Parent.ConnectionStringExpression.GetValueAsync(ct);
                var client = new ServiceBusClient(connectionString);
                var sender = client.CreateSender(resource.QueueName);

                var readyEvent = new Events.ServiceBusQueueReadyEvent(resource, sender);
                await builder.ApplicationBuilder.Eventing.PublishAsync(readyEvent, ct);
            });
        }
        return builder.OnEvent((AzureServiceBusQueueResource resource, Events.ServiceBusQueueReadyEvent evt, CancellationToken ct) => handler(evt, ct));
    }

    /// <summary>
    /// Registers a callback that will be invoked when the Service Bus topic sender is initialized and ready.
    /// </summary>
    /// <param name="builder">The resource builder for the Azure Service Bus topic resource.</param>
    /// <param name="handler">The callback to invoke when the topic sender is ready.</param>
    /// <returns>The resource builder for chaining.</returns>
    /// <remarks>
    /// This extension method subscribes to the resource ready event and creates a ServiceBusSender
    /// from the parent ServiceBusClient. The handler is invoked with the initialized sender.
    /// </remarks>
    public static IResourceBuilder<AzureServiceBusTopicResource> OnClientReady(
        this IResourceBuilder<AzureServiceBusTopicResource> builder,
        Func<Events.ServiceBusTopicReadyEvent, CancellationToken, Task> handler)
    {
        if (!builder.Resource.TryGetLastAnnotation<ServiceBusTopicReadyAnnotation>(out var existingEvent))
        {
            builder.WithAnnotation(new ServiceBusTopicReadyAnnotation());
            builder.OnResourceReady(async (resource, evt, ct) =>
            {
                var connectionString = await resource.Parent.ConnectionStringExpression.GetValueAsync(ct);
                var client = new ServiceBusClient(connectionString);
                var sender = client.CreateSender(resource.TopicName);

                var readyEvent = new Events.ServiceBusTopicReadyEvent(resource, sender);
                await builder.ApplicationBuilder.Eventing.PublishAsync(readyEvent, ct);
            });
        }
        return builder.OnEvent((AzureServiceBusTopicResource resource, Events.ServiceBusTopicReadyEvent evt, CancellationToken ct) => handler(evt, ct));
    }

    /// <summary>
    /// Registers a callback that will be invoked when the Service Bus subscription receiver is initialized and ready.
    /// </summary>
    /// <param name="builder">The resource builder for the Azure Service Bus subscription resource.</param>
    /// <param name="handler">The callback to invoke when the subscription receiver is ready.</param>
    /// <returns>The resource builder for chaining.</returns>
    /// <remarks>
    /// This extension method subscribes to the resource ready event and creates a ServiceBusReceiver
    /// from the parent ServiceBusClient. The handler is invoked with the initialized receiver.
    /// </remarks>
    public static IResourceBuilder<AzureServiceBusSubscriptionResource> OnClientReady(
        this IResourceBuilder<AzureServiceBusSubscriptionResource> builder,
        Func<Events.ServiceBusSubscriptionReadyEvent, CancellationToken, Task> handler)
    {
        if (!builder.Resource.TryGetLastAnnotation<ServiceBusSubscriptionReadyAnnotation>(out var existingEvent))
        {
            builder.WithAnnotation(new ServiceBusSubscriptionReadyAnnotation());
            builder.OnResourceReady(async (resource, evt, ct) =>
            {
                var connectionString = await resource.Parent.Parent.ConnectionStringExpression.GetValueAsync(ct);
                var client = new ServiceBusClient(connectionString);
                var receiver = client.CreateReceiver(resource.Parent.TopicName, resource.SubscriptionName);

                var readyEvent = new Events.ServiceBusSubscriptionReadyEvent(resource, receiver);
                await builder.ApplicationBuilder.Eventing.PublishAsync(readyEvent, ct);
            });
        }
        return builder.OnEvent((AzureServiceBusSubscriptionResource resource, Events.ServiceBusSubscriptionReadyEvent evt, CancellationToken ct) => handler(evt, ct));
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

    private class ServiceBusClientReadyAnnotation : IResourceAnnotation
    {
    }

    private class ServiceBusQueueReadyAnnotation : IResourceAnnotation
    {
    }

    private class ServiceBusTopicReadyAnnotation : IResourceAnnotation
    {
    }

    private class ServiceBusSubscriptionReadyAnnotation : IResourceAnnotation
    {
    }
}
