# Meir.Aspire.Azure.CosmosDB.ResourceClientReady

Resource client ready extensions for Azure Cosmos DB in .NET Aspire. Provides `OnClientReady` callbacks that execute when CosmosDB resources are initialized.

## Installation

```bash
dotnet add package Meir.Aspire.Azure.CosmosDB.ResourceClientReady
```

## Quick Start

```csharp
using Aspire.Hosting.Azure;
using Meir.Aspire.Azure.CosmosDB.ResourceClientReady;

var builder = DistributedApplication.CreateBuilder(args);

var cosmos = builder.AddAzureCosmosDB("cosmos-db")
    .RunAsEmulator();

// Subscribe to client ready event
cosmos.OnClientReady(async (evt, ct) =>
{
    var client = evt.Client;
    Console.WriteLine($"CosmosDB Client ready: {client.Endpoint}");
});

var database = cosmos.AddCosmosDatabase("my-database");

// Subscribe to database ready event
database.OnClientReady(async (evt, ct) =>
{
    var db = evt.Database;
    Console.WriteLine($"Database ready: {db.Id}");
});

var container = database.AddContainer("my-container", "/partitionKey");

// Subscribe to container ready event
container.OnClientReady(async (evt, ct) =>
{
    var container = evt.Container;
    // Perform initialization tasks
    await container.CreateItemAsync(new { id = "1", data = "sample" }, cancellationToken: ct);
});

builder.Build().Run();
```

## Features

### Client-Level Ready Event
Get notified when the CosmosClient is initialized:

```csharp
cosmos.OnClientReady(async (evt, ct) =>
{
    var client = evt.Client;
    // Access the CosmosClient instance
});
```

### Database-Level Ready Event
Get notified when a Database is ready:

```csharp
database.OnClientReady(async (evt, ct) =>
{
    var database = evt.Database;
    // Create containers, manage throughput, etc.
});
```

### Container-Level Ready Event
Get notified when a Container is ready:

```csharp
container.OnClientReady(async (evt, ct) =>
{
    var container = evt.Container;
    // Seed data, create indexes, etc.
});
```

## Usage Scenarios

### Seeding Data
```csharp
container.OnClientReady(async (evt, ct) =>
{
    var container = evt.Container;
    
    var seedData = new[]
    {
        new { id = "1", partitionKey = "pk1", name = "Item 1" },
        new { id = "2", partitionKey = "pk2", name = "Item 2" }
    };
    
    foreach (var item in seedData)
    {
        await container.UpsertItemAsync(item, cancellationToken: ct);
    }
});
```

### Creating Indexes
```csharp
database.OnClientReady(async (evt, ct) =>
{
    var database = evt.Database;
    
    var containerProperties = new ContainerProperties
    {
        Id = "indexed-container",
        PartitionKeyPath = "/category",
        IndexingPolicy = new IndexingPolicy
        {
            Automatic = true,
            IndexingMode = IndexingMode.Consistent
        }
    };
    
    await database.CreateContainerIfNotExistsAsync(containerProperties, cancellationToken: ct);
});
```

## API Reference

### Extension Methods

#### `OnClientReady(IResourceBuilder<AzureCosmosDBResource>)`
Registers a callback for when the Cosmos DB client is ready.

**Parameters:**
- `handler`: `Func<CosmosDBClientReadyEvent, CancellationToken, Task>` - Callback to invoke

**Returns:** The resource builder for chaining

#### `OnClientReady(IResourceBuilder<AzureCosmosDBDatabaseResource>)`
Registers a callback for when the database is ready.

**Parameters:**
- `handler`: `Func<CosmosDatabaseReadyEvent, CancellationToken, Task>` - Callback to invoke

**Returns:** The resource builder for chaining

#### `OnClientReady(IResourceBuilder<AzureCosmosDBContainerResource>)`
Registers a callback for when the container is ready.

**Parameters:**
- `handler`: `Func<CosmosContainerReadyEvent, CancellationToken, Task>` - Callback to invoke

**Returns:** The resource builder for chaining

### Events

#### `CosmosDBClientReadyEvent`
- `Resource`: The Azure Cosmos DB resource
- `Client`: The initialized CosmosClient

#### `CosmosDatabaseReadyEvent`
- `Resource`: The Azure Cosmos DB resource
- `Database`: The initialized Database

#### `CosmosContainerReadyEvent`
- `Resource`: The Azure Cosmos DB container resource
- `Container`: The initialized Container

## Contributing

Contributions are welcome! Please see the [main repository](https://github.com/Meir017/aspirations-for-aspire) for contribution guidelines.

## License

MIT License - see [LICENSE](../../LICENSE) for details.
