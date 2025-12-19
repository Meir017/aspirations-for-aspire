using Azure.Messaging.ServiceBus;
using Meir.Aspire.Azure.ServiceBus.ResourceClientReady;

var builder = DistributedApplication.CreateBuilder(args);

// Add Azure Service Bus with emulator
var serviceBus = builder.AddAzureServiceBus("demoservicebus")
    .RunAsEmulator()
    .OnClientReady(async (evt, ct) =>
    {
        Console.WriteLine($"[Service Bus] Client is ready for namespace: {evt.Resource.Name}");
        Console.WriteLine($"[Service Bus] Client can be used for administrative operations");
    });

// Add a queue with OnClientReady callback
var queue = serviceBus.AddServiceBusQueue("orders")
    .OnClientReady(async (evt, ct) =>
    {
        Console.WriteLine($"[Queue] Sender is ready for queue: {evt.Resource.QueueName}");
        
        // Send a test message
        var message = new ServiceBusMessage("Order #12345 created at " + DateTime.UtcNow);
        await evt.Sender.SendMessageAsync(message, ct);
        Console.WriteLine($"[Queue] Sent test message to queue: {evt.Resource.QueueName}");
    });

// Add a topic with OnClientReady callback
var topic = serviceBus.AddServiceBusTopic("notifications")
    .OnClientReady(async (evt, ct) =>
    {
        Console.WriteLine($"[Topic] Sender is ready for topic: {evt.Resource.TopicName}");
        
        // Publish a test message
        var message = new ServiceBusMessage("New notification at " + DateTime.UtcNow);
        message.ApplicationProperties.Add("Type", "Info");
        await evt.Sender.SendMessageAsync(message, ct);
        Console.WriteLine($"[Topic] Published test message to topic: {evt.Resource.TopicName}");
    });

// Add subscriptions to the topic
var emailSubscription = topic.AddServiceBusSubscription("email-alerts")
    .OnClientReady(async (evt, ct) =>
    {
        Console.WriteLine($"[Subscription] Receiver is ready for subscription: {evt.Resource.SubscriptionName}");
        
        // Peek at a message (non-destructive read)
        var message = await evt.Receiver.PeekMessageAsync(cancellationToken: ct);
        if (message != null)
        {
            Console.WriteLine($"[Subscription] Peeked message from subscription '{evt.Resource.SubscriptionName}': {message.Body}");
        }
        else
        {
            Console.WriteLine($"[Subscription] No messages available in subscription: {evt.Resource.SubscriptionName}");
        }
    });

var smsSubscription = topic.AddServiceBusSubscription("sms-alerts")
    .OnClientReady(async (evt, ct) =>
    {
        Console.WriteLine($"[Subscription] Receiver is ready for subscription: {evt.Resource.SubscriptionName}");
        
        // Peek at a message
        var message = await evt.Receiver.PeekMessageAsync(cancellationToken: ct);
        if (message != null)
        {
            Console.WriteLine($"[Subscription] Peeked message from subscription '{evt.Resource.SubscriptionName}': {message.Body}");
        }
        else
        {
            Console.WriteLine($"[Subscription] No messages available in subscription: {evt.Resource.SubscriptionName}");
        }
    });

builder.Build().Run();
