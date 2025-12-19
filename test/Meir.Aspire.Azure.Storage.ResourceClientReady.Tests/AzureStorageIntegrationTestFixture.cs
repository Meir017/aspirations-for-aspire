using Meir.Aspire.Testing;

namespace Meir.Aspire.Azure.Storage.ResourceClientReady.Tests;

public class AzureStorageIntegrationTestFixture : AspireIntegrationTestFixture<Projects.AzureStorage_AppHost>
{
    public bool BlobContainerClientReadyFired { get; private set; }

    protected override void OnBuilderCreated(DistributedApplicationBuilder applicationBuilder)
    {
        // Subscribe to Azure Storage client ready event to track when it fires
        applicationBuilder.Eventing.Subscribe<BlobContainerClientReadyEvent>((evt, ct) =>
        {
            BlobContainerClientReadyFired = true;
            return Task.CompletedTask;
        });

        base.OnBuilderCreated(applicationBuilder);
    }
}
