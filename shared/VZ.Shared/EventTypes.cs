namespace VZ.Shared
{
    public class EventTypes
    {
        public static class Audio
        {
            public const string AudioCreated = nameof(AudioCreated);
            public const string AudioDeleted = nameof(AudioDeleted);
            public const string AudioTranscriptUpdated = nameof(AudioTranscriptUpdated);
        }
        
        public static class Reviews
        {
            public const string BusinessReviewCreated = nameof(BusinessReviewCreated);
            public const string ReviewDeleted = nameof(ReviewDeleted);
            public const string ReviewUpdated = nameof(ReviewUpdated);
            public const string ReviewNameUpdated = nameof(ReviewNameUpdated);
            public const string ReviewImageUpdated = nameof(ReviewImageUpdated);
            public const string ReviewSynonymsUpdated = nameof(ReviewSynonymsUpdated);
            public const string ReviewItemsUpdated = nameof(ReviewItemsUpdated);
            public const string UploadBlockchain = nameof(UploadBlockchain);
            public const string BlockchainUploadComplete = nameof(BlockchainUploadComplete);
        }

        public static class Products
        {
            public const string ProductViewed = nameof(ProductViewed);
        }

        public static class Orders
        {
            public const string NewBusinessOrderCreated = nameof(NewBusinessOrderCreated);
        }
        
        public static class Images
        {
            //public const string ImageCaptionUpdated = nameof(ImageCaptionUpdated);
            //public const string ImageCreated = nameof(ImageCreated);
            //public const string ImageDeleted = nameof(ImageDeleted);

            public const string ImageThumbnailCreated = nameof(ImageThumbnailCreated);
        }
        
        public static class Microsoft
        {
            public static class Storage
            {
                public const string BlobCreated = "Microsoft.Storage.BlobCreated";
            }
        }

        public static class Widgets
        {
            public const string WidgetViewed = nameof(WidgetViewed);
        }
    }
}
