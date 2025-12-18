# Meir.Aspire.Azure.Storage.ResourceClientReady

Resource client ready extensions for Azure Storage in .NET Aspire. Provides `OnClientReady` callbacks that execute when Azure Storage resources are initialized.

## Installation

```bash
dotnet add package Meir.Aspire.Azure.Storage.ResourceClientReady
```

## Quick Start

```csharp
using Aspire.Hosting.Azure;
using Meir.Aspire.Azure.Storage.ResourceClientReady;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("azure-storage")
    .RunAsEmulator();

var blobs = storage.AddBlobs("my-container");

// Subscribe to blob container ready event
blobs.OnClientReady(async (evt, ct) =>
{
    var client = evt.Client;
    Console.WriteLine($"Blob container ready: {client.Name}");
    
    // Upload a sample blob
    await client.UploadBlobAsync(
        "sample.txt", 
        BinaryData.FromString("Hello, World!"), 
        cancellationToken: ct);
});

builder.Build().Run();
```

## Features

### Blob Container Ready Event
Get notified when the BlobContainerClient is initialized and the container is created:

```csharp
blobs.OnClientReady(async (evt, ct) =>
{
    var client = evt.Client;
    // The container is already created and ready to use
});
```

## Usage Scenarios

### Uploading Initial Files
```csharp
blobs.OnClientReady(async (evt, ct) =>
{
    var client = evt.Client;
    
    var files = new[]
    {
        ("readme.txt", "This is a sample file"),
        ("config.json", "{\"setting\": \"value\"}")
    };
    
    foreach (var (name, content) in files)
    {
        await client.UploadBlobAsync(
            name, 
            BinaryData.FromString(content), 
            cancellationToken: ct);
    }
});
```

### Setting Container Metadata
```csharp
blobs.OnClientReady(async (evt, ct) =>
{
    var client = evt.Client;
    
    var metadata = new Dictionary<string, string>
    {
        { "environment", "development" },
        { "initialized", DateTime.UtcNow.ToString("o") }
    };
    
    await client.SetMetadataAsync(metadata, cancellationToken: ct);
});
```

### Listing Blobs
```csharp
blobs.OnClientReady(async (evt, ct) =>
{
    var client = evt.Client;
    
    var blobs = client.GetBlobsAsync(cancellationToken: ct);
    
    await foreach (var blob in blobs)
    {
        Console.WriteLine($"Blob: {blob.Name} ({blob.Properties.ContentLength} bytes)");
    }
});
```

### Setting Access Policy
```csharp
blobs.OnClientReady(async (evt, ct) =>
{
    var client = evt.Client;
    
    // Set container to allow public read access to blobs
    await client.SetAccessPolicyAsync(
        Azure.Storage.Blobs.Models.PublicAccessType.Blob, 
        cancellationToken: ct);
});
```

## API Reference

### Extension Methods

#### `OnClientReady(IResourceBuilder<AzureBlobStorageResource>)`
Registers a callback for when the blob container client is ready.

**Parameters:**
- `handler`: `Func<BlobContainerClientReadyEvent, CancellationToken, Task>` - Callback to invoke

**Returns:** The resource builder for chaining

**Notes:**
- The blob container is automatically created if it doesn't exist
- The handler is invoked after the container is created

### Events

#### `BlobContainerClientReadyEvent`
- `Resource`: The Azure Blob Storage resource
- `Client`: The initialized BlobContainerClient

## Contributing

Contributions are welcome! Please see the [main repository](https://github.com/Meir017/aspirations-for-aspire) for contribution guidelines.

## License

MIT License - see [LICENSE](../../LICENSE) for details.
