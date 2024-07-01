using Dawn;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VZ.Reviews.Services.Models.Data
{
    public class Review : Shared.Models.BaseModel
    {
        public Review()
        {
            images = new ReviewImageList();
            tags = new List<string>();
        }

        // [Required]
        public string productId { get; set; }

        public string tamperproof { get; set; }

        /// <summary>
        /// Unix timestamp of when the tamperproof hash was created
        /// </summary>
        public long tamperproofedOn { get; set; }

        public bool verified { get; set; }

        
        // [JsonConverter(typeof(UnixDateTimeConverter))]
        /// <summary>
        /// Unix timestamp of when it was purchased
        /// </summary>
        public long purchasedOn { get; set; }

        [Range(0.1, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public decimal rating { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string title { get; set; }

        public string body { get; set; }

        [Required]
        public ReviewUser user { get; set; }

        public short upVotes { get; set; }

        public short downVotes { get; set; }

        public ReviewState state { get; set; }

        public ReviewStatus status
        {
            get => _status;
            set
            {
                //// Don't allow ready status if text and images have not been reviewed
                //if (_status != ReviewStatus.New && value == ReviewStatus.ReadyForPublic)
                //{
                //    if (!state.HasFlag(ReviewState.TextReviewed)) return;
                //    if (images.Count > 0 && !state.HasFlag(ReviewState.ImagesReviewed)) return;
                //}
                _status = value;
            }
        }
        private ReviewStatus _status;

        /// <summary>
        /// List of the previous reviews that were waiting to be linked to another review
        /// </summary>
        public List<string> previousReviewIds { get; set; }

        /// <summary>
        /// Id of the next review in the ledger
        /// </summary>
        public string nextReviewId { get; set; }

        public ReviewImageList images { get; set; }

        public List<string> tags { get; set; }

        public bool hasReplies { get; set; }

        public string GetHash()
        {
            Guard.Argument(title).NotEmpty();
            // body can be empty
            Guard.Argument(user.id).NotEqual(0);
            Guard.Argument(productId).NotEmpty();
            Guard.Argument(previousReviewIds).NotEmpty();

            string input = JsonConvert.SerializeObject(new
            {
                title,
                body,
                user.id,
                productId,
                previousReviewIds
            }, Formatting.None);
            return Shared.Security.Hasher.GetHash(input);
        }
    }

    public class ReviewUser
    {
        public long id { get; set; }

        public long businessCustomerId { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        public string avatarUrl { get; set; }
    }

    public class ReviewImage
    {
        public string name { get; set; }
        public string fullpath { get; set; }
        public string thumbpath { get; set; }

        public ReviewImage()
        {
        }

        public ReviewImage(string fullSizeUrl)
        {
            // review-media/reviewRequestId/
            var uri = new Uri(fullSizeUrl);
            name = uri.AbsolutePath.Substring(uri.AbsolutePath.LastIndexOf('/')).Trim('/');
            fullpath = uri.AbsolutePath.Replace("/" + Shared.Config.Blob.ReviewMediaContainer + "/", "");
        }
    }

    public enum ReviewStatus
    {
        New = 0,
        ReadyForBlockchain,
        ReadyForPublic
    }

    [Flags]
    public enum ReviewState
    {
        TextReviewed = 1,
        ImagesReviewed = 2
    }
}
