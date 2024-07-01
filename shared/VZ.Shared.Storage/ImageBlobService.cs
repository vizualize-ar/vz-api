using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VZ.Shared.Storage
{
    public class ImageBlobService
    {
        readonly ILogger _log = null;

        public ImageBlobService(ILogger log)
        {
            _log = log;
        }

        /// <summary>
        /// Takes an image input stream and creates a thumbnail for it and uploads it to the specified blob path
        /// </summary>
        /// <param name="inputImage"></param>
        /// <param name="containerName"></param>
        /// <param name="blobFolderName"></param>
        /// <param name="blobName"></param>
        /// <returns></returns>
        public async Task<Uri> GenerateThumbnail(Stream inputImage, string containerName, string blobFolderName, string blobName)
        {
            if (blobFolderName.EndsWith("/t") == false)
            {
                blobFolderName += "/t";
            }

            var extension = Path.GetExtension(blobName);
            var encoder = GetEncoder(extension);

            if (encoder == null)
            {
                _log.LogInformation($"No encoder support for: {containerName}, {blobFolderName}, {blobName}");
                return null;
            }

            IBlobService blobService = new BlobService(_log);
            int thumbnailWidth = Shared.Config.ThumbnailWidth;

            using (var output = new MemoryStream())
            // using (Image<Rgba32> image = Image.Load(inputImage))
            using (var image = Image.Load(inputImage))
            {
                var divisor = image.Width / thumbnailWidth;
                var height = Convert.ToInt32(Math.Round((decimal)(image.Height / divisor)));

                image.Mutate(x => x.Resize(thumbnailWidth, height));
                image.Save(output, encoder);
                output.Position = 0;

                var blob = await blobService.UploadBlobAsync(containerName, blobFolderName, blobName, output);
                return blob.Uri;
            }
        }

        public async Task<Uri> GenerateThumbnail(string inputBlobPath, string containerName, string blobFolderName, string blobName)
        {
            if (blobFolderName.EndsWith("/t") == false)
            {
                blobFolderName += "/t";
            }

            var extension = Path.GetExtension(blobName);
            var encoder = GetEncoder(extension);

            if (encoder == null)
            {
                _log.LogInformation($"No encoder support for: {containerName}, {blobFolderName}, {blobName}");
                return null;
            }

            IBlobService blobService = new BlobService(_log);
            int thumbnailWidth = Shared.Config.ThumbnailWidth;

            var inputBytes = await blobService.GetBlobBytesAsync(containerName, inputBlobPath);

            using (var output = new MemoryStream())
            using (Image<Rgba32> image = Image.Load(inputBytes))
            {
                var divisor = image.Width / thumbnailWidth;
                var height = Convert.ToInt32(Math.Round((decimal)(image.Height / divisor)));

                image.Mutate(x => x.Resize(thumbnailWidth, height));
                image.Save(output, encoder);
                output.Position = 0;

                var blob = await blobService.UploadBlobAsync(containerName, blobFolderName, blobName, output);
                return blob.Uri;
            }
        }

        //private static (string containerName, string blobFolderName, string blobName) GetNamesFromUrl(string blobUrl)
        //{
        //    var (blobPath, containerName) = BlobService.GetNamesFromUrl(blobUrl);

        //    var parts = blobPath.Split('/');
        //    string blobName = parts[parts.Length - 1];
        //    string blobFolderName = string.Join('/', parts.Take(parts.Length - 1));
        //    blobFolderName += "/t";

        //    return (containerName, blobFolderName, blobName);
        //}

        private static IImageEncoder GetEncoder(string extension)
        {
            IImageEncoder encoder = null;

            extension = extension.Replace(".", "");

            var isSupported = Regex.IsMatch(extension, "gif|png|jpe?g", RegexOptions.IgnoreCase);

            if (isSupported)
            {
                switch (extension)
                {
                    case "png":
                        encoder = new PngEncoder();
                        break;
                    case "jpg":
                        encoder = new JpegEncoder();
                        break;
                    case "jpeg":
                        encoder = new JpegEncoder();
                        break;
                    case "gif":
                        encoder = new GifEncoder();
                        break;
                    default:
                        break;
                }
            }

            return encoder;
        }
    }
}
