namespace Meir.Aspire.Azure.ServiceBus.ResourceClientReady.Tests;

public class ServiceBusIntegrationTestFixture() : AspireIntegrationTestFixture<ServiceBusIntegrationTestFixture.TestProgram>
{
    public class TestProgram { }

    public bool ClientEventFired { get; private set; }
    public bool QueueEventFired { get; private set; }
    public bool TopicEventFired { get; private set; }
    public bool SubscriptionEventFired { get; private set; }

    protected override void OnBuilderCreated(DistributedApplicationBuilder builder)
    {
        var serviceBus = builder.AddAzureServiceBus("testservicebus")
            .RunAsEmulator()
            .OnClientReady(async (evt, ct) =>
            {
                ClientEventFired = true;
                await Task.CompletedTask;
            });

        var queue = serviceBus.AddServiceBusQueue("testqueue")
            .OnClientReady(async (evt, ct) =>
            {
                QueueEventFired = true;
                await Task.CompletedTask;
            });

        var topic = serviceBus.AddServiceBusTopic("testtopic")
            .OnClientReady(async (evt, ct) =>
            {
                TopicEventFired = true;
                await Task.CompletedTask;
            });

        var subscription = topic.AddServiceBusSubscription("testsubscription")
            .OnClientReady(async (evt, ct) =>
            {
                SubscriptionEventFired = true;
                await Task.CompletedTask;
            });

        base.OnBuilderCreated(builder);
    }
}
