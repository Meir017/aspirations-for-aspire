using Aspire.Hosting;
using Meir.Aspire.Testing;

namespace Meir.Aspire.Azure.ServiceBus.ResourceClientReady.Tests;

public class ServiceBusIntegrationTestFixture : AspireIntegrationTestFixture<Projects.ServiceBus_AppHost>
{
    public bool ClientEventFired { get; private set; }
    public bool QueueEventFired { get; private set; }
    public bool TopicEventFired { get; private set; }
    public bool SubscriptionEventFired { get; private set; }

    protected override void OnBuilderCreated(DistributedApplicationBuilder applicationBuilder)
    {
        // Subscribe to Service Bus client ready events to track when they fire
        applicationBuilder.Eventing.Subscribe<Events.ServiceBusClientReadyEvent>((evt, ct) =>
        {
            ClientEventFired = true;
            return Task.CompletedTask;
        });

        applicationBuilder.Eventing.Subscribe<Events.ServiceBusQueueReadyEvent>((evt, ct) =>
        {
            QueueEventFired = true;
            return Task.CompletedTask;
        });

        applicationBuilder.Eventing.Subscribe<Events.ServiceBusTopicReadyEvent>((evt, ct) =>
        {
            TopicEventFired = true;
            return Task.CompletedTask;
        });

        applicationBuilder.Eventing.Subscribe<Events.ServiceBusSubscriptionReadyEvent>((evt, ct) =>
        {
            SubscriptionEventFired = true;
            return Task.CompletedTask;
        });

        base.OnBuilderCreated(applicationBuilder);
    }
}
