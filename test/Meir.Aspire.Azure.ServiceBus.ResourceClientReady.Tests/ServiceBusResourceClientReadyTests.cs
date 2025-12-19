using Aspire.Hosting.Azure;
using Aspire.Hosting.ApplicationModel;

namespace Meir.Aspire.Azure.ServiceBus.ResourceClientReady.Tests;

public class ServiceBusResourceClientReadyTests(ServiceBusIntegrationTestFixture fixture) 
    : IClassFixture<ServiceBusIntegrationTestFixture>
{
    [Fact]
    public async Task ServiceBusClient_OnClientReady_EventIsFired()
    {
        // Arrange & Act
        var serviceBusResource = fixture.App.Services.GetRequiredService<DistributedApplicationModel>()
            .Resources.OfType<AzureServiceBusResource>()
            .Single(r => r.Name == "demoservicebus");

        await fixture.ResourceNotificationService.WaitForResourceHealthyAsync(serviceBusResource.Name)
            .WaitAsync(TimeSpan.FromMinutes(5));

        // Assert
        Assert.True(fixture.ClientEventFired, "Service Bus client ready event should have fired");
    }

    [Fact]
    public async Task ServiceBusQueue_OnClientReady_EventIsFired()
    {
        // Arrange & Act
        var queueResource = fixture.App.Services.GetRequiredService<DistributedApplicationModel>()
            .Resources.OfType<AzureServiceBusQueueResource>()
            .Single(r => r.Name == "orders");

        await fixture.ResourceNotificationService.WaitForResourceHealthyAsync(queueResource.Parent.Name)
            .WaitAsync(TimeSpan.FromMinutes(5));

        // Give some time for event propagation
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Assert
        Assert.True(fixture.QueueEventFired, "Queue ready event should have fired");
    }

    [Fact]
    public async Task ServiceBusTopic_OnClientReady_EventIsFired()
    {
        // Arrange & Act
        var topicResource = fixture.App.Services.GetRequiredService<DistributedApplicationModel>()
            .Resources.OfType<AzureServiceBusTopicResource>()
            .Single(r => r.Name == "notifications");

        await fixture.ResourceNotificationService.WaitForResourceHealthyAsync(topicResource.Parent.Name)
            .WaitAsync(TimeSpan.FromMinutes(5));

        // Give some time for event propagation
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Assert
        Assert.True(fixture.TopicEventFired, "Topic ready event should have fired");
    }

    [Fact]
    public async Task ServiceBusSubscription_OnClientReady_EventIsFired()
    {
        // Arrange & Act
        var subscriptionResource = fixture.App.Services.GetRequiredService<DistributedApplicationModel>()
            .Resources.OfType<AzureServiceBusSubscriptionResource>()
            .Single(r => r.Name == "email-alerts");

        await fixture.ResourceNotificationService.WaitForResourceHealthyAsync(subscriptionResource.Parent.Parent.Name)
            .WaitAsync(TimeSpan.FromMinutes(5));

        // Give some time for event propagation
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Assert
        Assert.True(fixture.SubscriptionEventFired, "Subscription ready event should have fired");
    }
}
