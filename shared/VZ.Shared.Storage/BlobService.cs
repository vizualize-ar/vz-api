using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;

namespace VZ.Shared.Storage
{
    public interface IBlobService
    {
        Task<bool> BlobExistsAsync(CloudBlockBlob blob);

        CloudBlockBlob CreatePlaceholderBlob(string containerName, string folderName, string blobId);
        Task DeleteBlobAsync(string containerName, string folderName, string blobId);

        Task DownloadBlobAsync(CloudBlockBlob blob, Stream stream);
        Task<CloudBlockBlob> GetBlobAsync(string containerName, string folderName, string blobId, bool includeAttributes = false);

        Task<byte[]> GetBlobBytesAsync(CloudBlockBlob blob);

        Task<byte[]> GetBlobBytesAsync(string containerName, string blobPath);

        string GetSasTokenForBlob(CloudBlockBlob blob, SharedAccessBlobPolicy sasPolicy);

        Task<IList<CloudBlockBlob>> ListBlobsInFolderAsync(string containerName, string folderName);

        Task<string> MoveBlobAsync(string sourceUrl, string sourceContainerName, string destContainerName, string destBlobName = null);

        Task<List<string>> MoveBlobsInFolderAsync(string sourceContainerName, string sourceFolderName, string targetContainerName, string targetFolderName);

        Task UpdateBlobMetadataAsync(CloudBlockBlob blob);

        Task<CloudBlockBlob> UploadBlobAsync(string containerName, string folderName, string blobId, Stream stream, string contentType = null);
    }

    public class BlobService : IBlobService
    {
        private ILogger _log;
        private static readonly CloudBlobClient BlobClient;
        private static readonly CloudStorageAccount StorageAccount = CloudStorageAccount.Parse(Config.Blob.ConnectionString);

        static BlobService()
        {
            // connect to Azure Storage
            BlobClient = StorageAccount.CreateCloudBlobClient();
        }

        public BlobService(ILogger log)
        {
            _log = log;
        }

        public Task<bool> BlobExistsAsync(CloudBlockBlob blob)
        {
            return blob.ExistsAsync();
        }

        public CloudBlockBlob CreatePlaceholderBlob(string containerName, string folderName, string blobId)
        {
            var container = BlobClient.GetContainerReference(containerName);
            var folder = container.GetDirectoryReference(folderName);
            var blob = folder.GetBlockBlobReference(blobId);

            return blob;
        }

        public Task DeleteBlobAsync(string containerName, string folderName, string blobId)
        {
            var container = BlobClient.GetContainerReference(containerName);
            var folder = container.GetDirectoryReference(folderName);
            var blob = folder.GetBlockBlobReference(blobId);
            return blob.DeleteIfExistsAsync();
        }

        public Task DownloadBlobAsync(CloudBlockBlob blob, Stream stream)
        {
            return blob.DownloadToStreamAsync(stream);
        }

        public async Task<CloudBlockBlob> GetBlobAsync(string containerName, string folderName, string blobId, bool includeAttributes = false)
        {
            var container = BlobClient.GetContainerReference(containerName);
            var folder = container.GetDirectoryReference(folderName);
            var blob = folder.GetBlockBlobReference(blobId);

            if (!await blob.ExistsAsync())
            {
                return null;
            }
            if (includeAttributes)
            {
                await blob.FetchAttributesAsync();
            }

            return blob;
        }

        public async Task<byte[]> GetBlobBytesAsync(CloudBlockBlob blob)
        {
            var bytes = new byte[blob.Properties.Length];
            await blob.DownloadToByteArrayAsync(bytes, 0);
            return bytes;
        }

        public async Task<byte[]> GetBlobBytesAsync(string containerName, string blobPath)
        {
            var (blobFolder, blobName) = GetNamesFromPath(blobPath);
            var blob = await GetBlobAsync(containerName, blobFolder, blobName);
            var bytes = new byte[blob.Properties.Length];
            await blob.DownloadToByteArrayAsync(bytes, 0);
            return bytes;
        }

        public string GetSasTokenForBlob(CloudBlockBlob blob, SharedAccessBlobPolicy sasPolicy)
        {
            var sasBlobToken = blob.GetSharedAccessSignature(sasPolicy);
            return blob.Uri + sasBlobToken;
        }

        public async Task<IList<CloudBlockBlob>> ListBlobsInFolderAsync(string containerName, string folderName)
        {
            var container = BlobClient.GetContainerReference(containerName);
            var folder = container.GetDirectoryReference(folderName);

            // list all blobs in folder
            var blobsInFolder = new List<CloudBlockBlob>();
            var continuationToken = new BlobContinuationToken();
            do
            {
                var currentPage = await folder.ListBlobsSegmentedAsync(true, BlobListingDetails.Metadata, 100, continuationToken, null, null);
                blobsInFolder.AddRange(currentPage.Results.OfType<CloudBlockBlob>());
                continuationToken = currentPage.ContinuationToken;
            } while (continuationToken != null);

            return blobsInFolder;
        }

        public async Task<string> MoveBlobAsync(string sourceUrl, string sourceContainerName, string destContainerName, string destBlobName = null)
        {
            CloudBlobContainer sourceContainer = BlobClient.GetContainerReference(sourceContainerName);
            CloudBlobContainer targetContainer = BlobClient.GetContainerReference(destContainerName);

            string blobName = GetNamesFromUrl(sourceUrl).blobName;
            CloudBlockBlob sourceBlob = sourceContainer.GetBlockBlobReference(blobName);
            CloudBlockBlob targetBlob = targetContainer.GetBlockBlobReference(destBlobName ?? blobName);

            await targetBlob.StartCopyAsync(sourceBlob);

            while (!targetBlob.Exists())
            {
                System.Threading.Thread.Sleep(500);
            }
            sourceBlob.Delete();

            return targetBlob.Uri.AbsoluteUri;
        }

        public async Task<List<string>> MoveBlobsInFolderAsync(string sourceContainerName, string sourceFolderName, string targetContainerName, string targetFolderName)
        {
            var sourceContainer = BlobClient.GetContainerReference(sourceContainerName);
            var sourceFolder = sourceContainer.GetDirectoryReference(sourceFolderName);

            // list all blobs in folder
            var blobsInSourceFolder = new List<CloudBlockBlob>();
            var continuationToken = new BlobContinuationToken();
            do
            {
                var currentPage = await sourceFolder.ListBlobsSegmentedAsync(true, BlobListingDetails.Metadata, 100, continuationToken, null, null);
                blobsInSourceFolder.AddRange(currentPage.Results.OfType<CloudBlockBlob>());
                continuationToken = currentPage.ContinuationToken;
            } while (continuationToken != null);

            List<string> targetUrls = new List<string>();
            foreach(var sourceBlob in blobsInSourceFolder)
            {
                var url = await MoveBlobAsync(sourceBlob.Uri.AbsoluteUri, sourceBlob.Container.Name, targetContainerName, targetFolderName + "/" + Path.GetFileName(sourceBlob.Name));
                targetUrls.Add(url);
            }
            
            return targetUrls;
        }

        public async Task UpdateBlobMetadataAsync(CloudBlockBlob blob)
        {
            var metadataUpdate = blob.SetMetadataAsync();
            var propertiesUpdate = blob.SetPropertiesAsync();
            await Task.WhenAll(metadataUpdate, propertiesUpdate);
        }

        public async Task<CloudBlockBlob> UploadBlobAsync(string containerName, string folderName, string blobId, Stream stream, string contentType = null)
        {
            var container = BlobClient.GetContainerReference(containerName);
            var folder = container.GetDirectoryReference(folderName);
            var blob = folder.GetBlockBlobReference(blobId);

            // upload blob
            if (!string.IsNullOrWhiteSpace(contentType))
            {
                blob.Properties.ContentType = contentType;
            }
            await blob.UploadFromStreamAsync(stream);

            return blob;
        }

        public static (string containerName, string blobFolderName, string blobName) GetNamesFromUrl(string bloblUrl)
        {
            var uri = new Uri(bloblUrl);
            var cloudBlob = new CloudBlob(uri);

            // return (cloudBlob.Container.Name, cloudBlob.Name);

            var blobPath = cloudBlob.Name;
            var containerName = cloudBlob.Container.Name;
            var names = GetNamesFromPath(blobPath);

            return (containerName, names.blobFolderName, names.blobName);
        }

        public static (string blobFolderName, string blobName) GetNamesFromPath(string blobPath)
        {
            var parts = blobPath.Split('/');
            string blobName = parts[parts.Length - 1];
            string blobFolderName = string.Join('/', parts.Take(parts.Length - 1));

            return (blobFolderName, blobName);
        }
    }
}
