using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Eventing;
using Azure.Messaging.ServiceBus;

namespace Meir.Aspire.Azure.ServiceBus.ResourceClientReady.Events;

/// <summary>
/// Event published when an Azure Service Bus subscription receiver is initialized and ready.
/// </summary>
/// <param name="Resource">The Azure Service Bus subscription resource.</param>
/// <param name="Receiver">The initialized ServiceBusReceiver instance for the subscription.</param>
public record ServiceBusSubscriptionReadyEvent(
    AzureServiceBusSubscriptionResource Resource,
    ServiceBusReceiver Receiver) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}
