using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Eventing;
using Azure.Messaging.ServiceBus;

namespace Meir.Aspire.Azure.ServiceBus.ResourceClientReady.Events;

/// <summary>
/// Event published when an Azure Service Bus client is initialized and ready.
/// </summary>
/// <param name="Resource">The Azure Service Bus resource.</param>
/// <param name="Client">The initialized ServiceBusClient instance.</param>
public record ServiceBusClientReadyEvent(
    AzureServiceBusResource Resource,
    ServiceBusClient Client) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}
