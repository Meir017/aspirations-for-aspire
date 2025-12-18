using Aspire.Hosting;
using Meir.Aspire.Azure.CosmosDB.ResourceClientReady;
using Meir.Aspire.Testing;

namespace Meir.Aspire.Azure.CosmosDB.ResourceClientReady.Tests;

public class CosmosDBIntegrationTestFixture : AspireIntegrationTestFixture<Projects.CosmosDB_AppHost>
{
    public bool CosmosClientReadyFired { get; private set; }
    public bool CosmosDatabaseReadyFired { get; private set; }
    public bool CosmosContainerReadyFired { get; private set; }

    protected override void OnBuilderCreated(DistributedApplicationBuilder applicationBuilder)
    {
        // Subscribe to CosmosDB client ready events to track when they fire
        applicationBuilder.Eventing.Subscribe<CosmosDBClientReadyEvent>((evt, ct) =>
        {
            CosmosClientReadyFired = true;
            return Task.CompletedTask;
        });

        applicationBuilder.Eventing.Subscribe<CosmosDatabaseReadyEvent>((evt, ct) =>
        {
            CosmosDatabaseReadyFired = true;
            return Task.CompletedTask;
        });

        applicationBuilder.Eventing.Subscribe<CosmosContainerReadyEvent>((evt, ct) =>
        {
            CosmosContainerReadyFired = true;
            return Task.CompletedTask;
        });

        base.OnBuilderCreated(applicationBuilder);
    }
}
