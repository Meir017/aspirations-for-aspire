using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Eventing;
using Azure.Messaging.ServiceBus;

namespace Meir.Aspire.Azure.ServiceBus.ResourceClientReady.Events;

/// <summary>
/// Event published when an Azure Service Bus queue sender is initialized and ready.
/// </summary>
/// <param name="Resource">The Azure Service Bus queue resource.</param>
/// <param name="Sender">The initialized ServiceBusSender instance for the queue.</param>
public record ServiceBusQueueReadyEvent(
    AzureServiceBusQueueResource Resource,
    ServiceBusSender Sender) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}
