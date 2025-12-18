using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Eventing;
using Microsoft.Azure.Cosmos;

namespace Meir.Aspire.Azure.CosmosDB.ResourceClientReady;

/// <summary>
/// Event raised when a Cosmos DB database is initialized and ready for use.
/// </summary>
/// <param name="Resource">The Azure Cosmos DB resource.</param>
/// <param name="Database">The initialized Database instance.</param>
public record class CosmosDatabaseReadyEvent(AzureCosmosDBResource Resource, Database Database) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}
