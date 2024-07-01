using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using TrueRevue.Shared.Storage;
using TrueRevue.Shared.EventSchemas.Images;

namespace TrueRevue.Imaging.WorkerApi
{
    public static class ThumbnailFunctions
    {
        [Shared.EventHandler(Shared.EventTypes.Microsoft.Storage.BlobCreated)]
        [FunctionName("GenerateThumbnail")]
        public static async Task GenerateThumbnail(
            [EventGridTrigger]EventGridEvent eventGridEvent,
            [Blob("{data.url}", FileAccess.Read, Connection = "BlobConnectionString")] Stream input,
            ILogger logger)
        {
            try
            {
                if (input == null)
                {
                    logger.LogCritical("No blob in event. EventGridEvent ID={0}", eventGridEvent.Id);
                    return;
                }
                var createdEvent = ((JObject)eventGridEvent.Data).ToObject<StorageBlobCreatedEventData>();

                if (createdEvent.Url.Contains("/t/"))
                {
                    // event fired for a generated thumbnail
                    return;
                }

                var imageBlobService = new ImageBlobService(logger);
                var (containerName, blobFolderName, blobName) = BlobService.GetNamesFromUrl(createdEvent.Url);
                await imageBlobService.GenerateThumbnail(input, containerName, blobFolderName, blobName);

                await PublishThumbnailCreatedEvent(containerName, string.Join('/', blobFolderName, blobName), logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating thumbnail for EventGridEvent. ID={0}", eventGridEvent.Id);
                throw;
            }
        }

        private static async Task PublishThumbnailCreatedEvent(string containerName, string thumbnailPath, ILogger log)
        {
            var eventGridPublisherService = new Shared.EventGridPublisherService(log);
            var payload = new ImageThumbnailCreatedEventData
            {
                thumbnailBlobPath = thumbnailPath
            };
            await eventGridPublisherService.PostEventGridEventAsync(Shared.EventTypes.Images.ImageThumbnailCreated, containerName, payload);
        }
    }
}
