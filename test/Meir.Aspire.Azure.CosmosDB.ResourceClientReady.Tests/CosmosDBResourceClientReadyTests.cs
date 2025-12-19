namespace Meir.Aspire.Azure.CosmosDB.ResourceClientReady.Tests;

public class CosmosDBResourceClientReadyTests(
    CosmosDBIntegrationTestFixture fixture)
    : IClassFixture<CosmosDBIntegrationTestFixture>
{
    [Fact]
    public async Task CosmosDB_AppHost_StartsSuccessfully_And_OnClientReady_Events_Fire()
    {
        // Arrange & Act - fixture initializes the app
        
        // Assert - verify CosmosDB resource becomes healthy
        await fixture.ResourceNotificationService
            .WaitForResourceHealthyAsync("cosmos-db")
            .WaitAsync(TimeSpan.FromMinutes(5));

        // Verify all OnClientReady events fired successfully
        Assert.True(fixture.CosmosClientReadyFired, "CosmosDB Client Ready event should have fired");
        Assert.True(fixture.CosmosDatabaseReadyFired, "CosmosDB Database Ready event should have fired");
        Assert.True(fixture.CosmosContainerReadyFired, "CosmosDB Container Ready event should have fired");
        Assert.NotNull(fixture.App);
    }
}
