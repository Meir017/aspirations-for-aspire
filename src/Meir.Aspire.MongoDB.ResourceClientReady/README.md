# Meir.Aspire.MongoDB.ResourceClientReady

Resource client ready extensions for MongoDB in .NET Aspire. Provides `OnClientReady` callbacks that execute when MongoDB resources are initialized.

## Installation

```bash
dotnet add package Meir.Aspire.MongoDB.ResourceClientReady
```

## Quick Start

```csharp
using Meir.Aspire.MongoDB.ResourceClientReady;
using MongoDB.Bson;

var builder = DistributedApplication.CreateBuilder(args);

var mongo = builder.AddMongoDB("mongo-db");

// Subscribe to client ready event
mongo.OnClientReady(async (evt, ct) =>
{
    var client = evt.Client;
    Console.WriteLine($"MongoDB Client ready: {client.Cluster.Description.ClusterId}");
});

var database = mongo.AddDatabase("my-database");

// Subscribe to database ready event
database.OnClientReady(async (evt, ct) =>
{
    var db = evt.Database;
    Console.WriteLine($"Database ready: {db.DatabaseNamespace.DatabaseName}");
    
    // Seed data
    var collection = db.GetCollection<BsonDocument>("my-collection");
    await collection.InsertOneAsync(
        new BsonDocument { { "message", "Hello, MongoDB!" } }, 
        cancellationToken: ct);
});

builder.Build().Run();
```

## Features

### Server-Level Ready Event
Get notified when the MongoDB client is initialized:

```csharp
mongo.OnClientReady(async (evt, ct) =>
{
    var client = evt.Client;
    // Access the MongoClient instance
});
```

### Database-Level Ready Event
Get notified when a Database is ready:

```csharp
database.OnClientReady(async (evt, ct) =>
{
    var database = evt.Database;
    // Create collections, seed data, etc.
});
```

## Usage Scenarios

### Seeding Data
```csharp
database.OnClientReady(async (evt, ct) =>
{
    var database = evt.Database;
    var collection = database.GetCollection<BsonDocument>("users");
    
    var users = new[]
    {
        new BsonDocument { { "name", "Alice" }, { "email", "alice@example.com" } },
        new BsonDocument { { "name", "Bob" }, { "email", "bob@example.com" } }
    };
    
    await collection.InsertManyAsync(users, cancellationToken: ct);
});
```

### Creating Indexes
```csharp
database.OnClientReady(async (evt, ct) =>
{
    var database = evt.Database;
    var collection = database.GetCollection<BsonDocument>("products");
    
    var indexKeys = Builders<BsonDocument>.IndexKeys
        .Ascending("category")
        .Descending("price");
    
    await collection.Indexes.CreateOneAsync(
        new CreateIndexModel<BsonDocument>(indexKeys),
        cancellationToken: ct);
});
```

### Collection Initialization
```csharp
database.OnClientReady(async (evt, ct) =>
{
    var database = evt.Database;
    
    // Create capped collection
    await database.CreateCollectionAsync(
        "logs",
        new CreateCollectionOptions 
        { 
            Capped = true, 
            MaxSize = 10485760 // 10 MB
        },
        cancellationToken: ct);
});
```

## API Reference

### Extension Methods

#### `OnClientReady(IResourceBuilder<MongoDBServerResource>)`
Registers a callback for when the MongoDB client is ready.

**Parameters:**
- `handler`: `Func<MongoClientReadyEvent, CancellationToken, Task>` - Callback to invoke

**Returns:** The resource builder for chaining

#### `OnClientReady(IResourceBuilder<MongoDBDatabaseResource>)`
Registers a callback for when the database is ready.

**Parameters:**
- `handler`: `Func<MongoDatabaseReadyEvent, CancellationToken, Task>` - Callback to invoke

**Returns:** The resource builder for chaining

### Events

#### `MongoClientReadyEvent`
- `Resource`: The MongoDB server resource
- `Client`: The initialized MongoClient

#### `MongoDatabaseReadyEvent`
- `Resource`: The MongoDB server resource
- `Database`: The initialized Database

## Contributing

Contributions are welcome! Please see the [main repository](https://github.com/Meir017/aspirations-for-aspire) for contribution guidelines.

## License

MIT License - see [LICENSE](../../LICENSE) for details.
