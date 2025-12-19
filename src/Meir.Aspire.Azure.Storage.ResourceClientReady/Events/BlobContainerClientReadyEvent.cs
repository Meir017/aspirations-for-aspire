using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Eventing;
using Azure.Storage.Blobs;

namespace Meir.Aspire.Azure.Storage.ResourceClientReady;

/// <summary>
/// Event raised when an Azure Blob Container client is initialized and ready for use.
/// </summary>
/// <param name="Resource">The Azure Blob Storage resource.</param>
/// <param name="Client">The initialized BlobContainerClient instance.</param>
public record class BlobContainerClientReadyEvent(AzureBlobStorageResource Resource, BlobContainerClient Client) : IDistributedApplicationResourceEvent
{
    IResource IDistributedApplicationResourceEvent.Resource => Resource;
}
