# Meir.Aspire.Azure.ServiceBus.ResourceClientReady

Provides event-driven resource initialization patterns for Azure Service Bus in .NET Aspire applications.

## Features

- **OnClientReady callbacks** for Azure Service Bus resources at all hierarchy levels:
  - Service Bus Namespace → `ServiceBusClient`
  - Queue → `ServiceBusSender`
  - Topic → `ServiceBusSender`
  - Subscription → `ServiceBusReceiver`

## Installation

```bash
dotnet add package Meir.Aspire.Azure.ServiceBus.ResourceClientReady
```

## Usage

### Service Bus Namespace Client Ready

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var serviceBus = builder.AddAzureServiceBus("myservicebus")
    .RunAsEmulator()
    .OnClientReady(async (evt, ct) =>
    {
        var client = evt.Client;
        // Use the ServiceBusClient for administrative operations
        Console.WriteLine("Service Bus client is ready!");
    });
```

### Queue Sender Ready

```csharp
var queue = serviceBus.AddServiceBusQueue("myqueue")
    .OnClientReady(async (evt, ct) =>
    {
        var sender = evt.Sender;
        // Send a test message
        await sender.SendMessageAsync(new ServiceBusMessage("Test message"), ct);
        Console.WriteLine($"Queue sender for '{evt.Resource.QueueName}' is ready!");
    });
```

### Topic Sender Ready

```csharp
var topic = serviceBus.AddServiceBusTopic("mytopic")
    .OnClientReady(async (evt, ct) =>
    {
        var sender = evt.Sender;
        // Publish a test message
        await sender.SendMessageAsync(new ServiceBusMessage("Topic message"), ct);
        Console.WriteLine($"Topic sender for '{evt.Resource.TopicName}' is ready!");
    });
```

### Subscription Receiver Ready

```csharp
var subscription = topic.AddServiceBusSubscription("mysubscription")
    .OnClientReady(async (evt, ct) =>
    {
        var receiver = evt.Receiver;
        // Peek at messages
        var message = await receiver.PeekMessageAsync(cancellationToken: ct);
        Console.WriteLine($"Subscription receiver for '{evt.Resource.SubscriptionName}' is ready!");
    });
```

## Architecture

This library follows the event-driven resource initialization pattern:

1. **Extension Methods** on `IResourceBuilder<T>` named `OnClientReady`
2. **Marker Annotations** prevent duplicate event subscriptions
3. **Event Records** carry initialized client instances
4. **Lifecycle Hooks** use `OnResourceReady` to initialize clients

## License

MIT
