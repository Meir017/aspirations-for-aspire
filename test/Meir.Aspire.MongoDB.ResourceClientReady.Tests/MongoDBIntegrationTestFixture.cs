using Aspire.Hosting;
using Meir.Aspire.MongoDB.ResourceClientReady;
using Meir.Aspire.Testing;

namespace Meir.Aspire.MongoDB.ResourceClientReady.Tests;

public class MongoDBIntegrationTestFixture : AspireIntegrationTestFixture<Projects.MongoDB_AppHost>
{
    public bool MongoClientReadyFired { get; private set; }
    public bool MongoDatabaseReadyFired { get; private set; }

    protected override void OnBuilderCreated(DistributedApplicationBuilder applicationBuilder)
    {
        // Subscribe to MongoDB client ready events to track when they fire
        applicationBuilder.Eventing.Subscribe<MongoClientReadyEvent>((evt, ct) =>
        {
            MongoClientReadyFired = true;
            return Task.CompletedTask;
        });

        applicationBuilder.Eventing.Subscribe<MongoDatabaseReadyEvent>((evt, ct) =>
        {
            MongoDatabaseReadyFired = true;
            return Task.CompletedTask;
        });

        base.OnBuilderCreated(applicationBuilder);
    }
}
