using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Eventing;
using MongoDB.Driver;

namespace Meir.Aspire.MongoDB.ResourceClientReady;

/// <summary>
/// Event raised when a MongoDB database is initialized and ready for use.
/// </summary>
/// <param name="Resource">The MongoDB server resource.</param>
/// <param name="Database">The initialized Database instance.</param>
public record class MongoDatabaseReadyEvent(MongoDBServerResource Resource, IMongoDatabase Database) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}
