# Aspirations for Aspire

A collection of .NET Aspire libraries that extend the framework with additional capabilities and patterns.

## 📦 Available Libraries

### Resource Client Ready Extensions

Execute callbacks when Aspire resources are fully initialized and ready to use. These libraries provide event-driven patterns for resource initialization.

#### [Meir.Aspire.Azure.CosmosDB.ResourceClientReady](src/Meir.Aspire.Azure.CosmosDB.ResourceClientReady/)
Resource client ready extensions for Azure Cosmos DB. Provides `OnClientReady` callbacks for CosmosDB clients, databases, and containers.

```bash
dotnet add package Meir.Aspire.Azure.CosmosDB.ResourceClientReady
```

#### [Meir.Aspire.MongoDB.ResourceClientReady](src/Meir.Aspire.MongoDB.ResourceClientReady/)
Resource client ready extensions for MongoDB. Provides `OnClientReady` callbacks for MongoDB clients and databases.

```bash
dotnet add package Meir.Aspire.MongoDB.ResourceClientReady
```

#### [Meir.Aspire.Azure.Storage.ResourceClientReady](src/Meir.Aspire.Azure.Storage.ResourceClientReady/)
Resource client ready extensions for Azure Storage. Provides `OnClientReady` callbacks for Azure Blob Storage containers.

```bash
dotnet add package Meir.Aspire.Azure.Storage.ResourceClientReady
```

## 🚀 Quick Start

### Example: CosmosDB Resource Client Ready

```csharp
using Meir.Aspire.Azure.CosmosDB.ResourceClientReady;

var builder = DistributedApplication.CreateBuilder(args);

var cosmos = builder.AddAzureCosmosDB("cosmos-db").RunAsEmulator();

// Execute code when the CosmosDB client is ready
cosmos.OnClientReady(async (evt, ct) =>
{
    var client = evt.Client;
    Console.WriteLine($"CosmosDB Client is ready - endpoint: {client.Endpoint}");
});

var database = cosmos.AddCosmosDatabase("my-database");

// Execute code when the database is ready
database.OnClientReady(async (evt, ct) =>
{
    var db = evt.Database;
    Console.WriteLine($"Database '{db.Id}' is ready");
});

builder.Build().Run();
```

## 🎯 Use Cases

- **Database Seeding**: Initialize databases with seed data when they become available
- **Schema Creation**: Create collections, containers, and indexes on startup
- **Health Checks**: Verify connectivity and perform readiness checks
- **Logging & Monitoring**: Track when resources are initialized
- **Integration Testing**: Hook into resource lifecycle in tests

## 📁 Repository Structure

```
aspirations-for-aspire/
├── src/                                    # Library projects
│   ├── Meir.Aspire.Azure.CosmosDB.ResourceClientReady/
│   ├── Meir.Aspire.MongoDB.ResourceClientReady/
│   └── Meir.Aspire.Azure.Storage.ResourceClientReady/
├── playground/                             # Sample applications
│   ├── CosmosDB.AppHost/
│   ├── MongoDB.AppHost/
│   └── AzureStorage.AppHost/
└── test/                                   # Integration tests
    ├── Meir.Aspire.Azure.CosmosDB.ResourceClientReady.Tests/
    ├── Meir.Aspire.MongoDB.ResourceClientReady.Tests/
    └── Meir.Aspire.Azure.Storage.ResourceClientReady.Tests/
```

## 🛠️ Requirements

- .NET 9.0 or later
- .NET Aspire 13.1.0 or later

## 📖 Documentation

Each library includes comprehensive documentation:
- [CosmosDB README](src/Meir.Aspire.Azure.CosmosDB.ResourceClientReady/README.md)
- [MongoDB README](src/Meir.Aspire.MongoDB.ResourceClientReady/README.md)
- [Azure Storage README](src/Meir.Aspire.Azure.Storage.ResourceClientReady/README.md)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🔗 Links

- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [GitHub Repository](https://github.com/Meir017/aspirations-for-aspire)
