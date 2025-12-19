using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Eventing;
using Azure.Messaging.ServiceBus;

namespace Meir.Aspire.Azure.ServiceBus.ResourceClientReady.Events;

/// <summary>
/// Event published when an Azure Service Bus topic sender is initialized and ready.
/// </summary>
/// <param name="Resource">The Azure Service Bus topic resource.</param>
/// <param name="Sender">The initialized ServiceBusSender instance for the topic.</param>
public record ServiceBusTopicReadyEvent(
    AzureServiceBusTopicResource Resource,
    ServiceBusSender Sender) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}
