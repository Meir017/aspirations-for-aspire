using Meir.Aspire.Azure.Storage.ResourceClientReady;

var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator()
    .AddBlobs("blobs");

storage.OnClientReady(async (BlobContainerClientReadyEvent evt, CancellationToken ct) =>
{
    var exists = await evt.Client.ExistsAsync(ct);
    Console.WriteLine($"Blob Container Ready - Container: {evt.Client.Name}, Exists: {exists.Value}");
});

builder.Build().Run();
