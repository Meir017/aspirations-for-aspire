using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Eventing;
using Microsoft.Azure.Cosmos;

namespace Meir.Aspire.Azure.CosmosDB.ResourceClientReady;

/// <summary>
/// Event raised when a Cosmos DB client is initialized and ready for use.
/// </summary>
/// <param name="Resource">The Azure Cosmos DB resource.</param>
/// <param name="Client">The initialized CosmosClient instance.</param>
public record class CosmosDBClientReadyEvent(AzureCosmosDBResource Resource, CosmosClient Client) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}
