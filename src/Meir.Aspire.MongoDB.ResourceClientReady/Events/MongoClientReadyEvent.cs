using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Eventing;
using MongoDB.Driver;

namespace Meir.Aspire.MongoDB.ResourceClientReady;

/// <summary>
/// Event raised when a MongoDB client is initialized and ready for use.
/// </summary>
/// <param name="Resource">The MongoDB server resource.</param>
/// <param name="Client">The initialized MongoClient instance.</param>
public record class MongoClientReadyEvent(MongoDBServerResource Resource, IMongoClient Client) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}
