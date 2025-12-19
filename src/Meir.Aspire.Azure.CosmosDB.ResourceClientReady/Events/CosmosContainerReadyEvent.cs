using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Eventing;
using Microsoft.Azure.Cosmos;

namespace Meir.Aspire.Azure.CosmosDB.ResourceClientReady;

/// <summary>
/// Event raised when a Cosmos DB container is initialized and ready for use.
/// </summary>
/// <param name="Resource">The Azure Cosmos DB container resource.</param>
/// <param name="Container">The initialized Container instance.</param>
public record class CosmosContainerReadyEvent(AzureCosmosDBContainerResource Resource, Container Container) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}
