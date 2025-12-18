using Xunit;

namespace Meir.Aspire.MongoDB.ResourceClientReady.Tests;

public class MongoDBResourceClientReadyTests(
    MongoDBIntegrationTestFixture fixture)
    : IClassFixture<MongoDBIntegrationTestFixture>
{
    [Fact]
    public async Task MongoDB_AppHost_StartsSuccessfully_And_OnClientReady_Events_Fire()
    {
        // Arrange & Act - fixture initializes the app
        
        // Assert - verify MongoDB resource becomes healthy
        await fixture.ResourceNotificationService
            .WaitForResourceHealthyAsync("mongodb")
            .WaitAsync(TimeSpan.FromMinutes(5));

        // Verify all OnClientReady events fired successfully
        Assert.True(fixture.MongoClientReadyFired, "MongoDB Client Ready event should have fired");
        Assert.True(fixture.MongoDatabaseReadyFired, "MongoDB Database Ready event should have fired");
        Assert.NotNull(fixture.App);
    }
}
