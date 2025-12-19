namespace Meir.Aspire.Azure.Storage.ResourceClientReady.Tests;

public class AzureStorageResourceClientReadyTests(
    AzureStorageIntegrationTestFixture fixture)
    : IClassFixture<AzureStorageIntegrationTestFixture>
{
    [Fact]
    public async Task AzureStorage_AppHost_StartsSuccessfully_And_OnClientReady_Events_Fire()
    {
        // Arrange & Act - fixture initializes the app
        
        // Assert - verify Azure Storage blob resource becomes healthy
        await fixture.ResourceNotificationService
            .WaitForResourceHealthyAsync("blobs")
            .WaitAsync(TimeSpan.FromMinutes(5));

        // Verify the OnClientReady event fired successfully
        Assert.True(fixture.BlobContainerClientReadyFired, "Blob Container Client Ready event should have fired");
        Assert.NotNull(fixture.App);
    }
}
