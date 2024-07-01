using System;
using System.IO;
using Microsoft.Azure.Storage.Blob;
using VZ.Shared.Storage;
using Dawn;
using Microsoft.Extensions.Logging;

namespace VZ.Reviews.Services
{
    public class ImageService
    {
        //private string _blobContainerName;

        //public ImageService(string blobContainerName)
        //{
        //    Guard.Argument(blobContainerName).NotEmpty().NotNull();

        //    _blobContainerName = blobContainerName;
        //}
        private ILogger _log;

        public ImageService(ILogger log)
        {
            _log = log;
        }

        public (string id, string url) BeginAddImage(string blobContainerName, string reviewRequestId, string productId, string placeHolderName)
        {
            // generate an ID for this image note
            var imageId = placeHolderName ?? Guid.NewGuid().ToString();

            string path = String.Format("{0}/{1}", reviewRequestId, productId);

            // create a blob placeholder (which will not have any contents yet)
            var blobRepository = new BlobService(_log);
            var blob = blobRepository.CreatePlaceholderBlob(blobContainerName, path, imageId);

            // get a SAS token to allow the client to write the blob
            var writePolicy = new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = DateTime.UtcNow.AddMinutes(-5), // to allow for clock skew
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(24),
                Permissions = SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Delete
            };
            var url = blobRepository.GetSasTokenForBlob(blob, writePolicy);

            return (imageId, url);
        }
    }
}
