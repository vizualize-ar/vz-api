using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VZ.Reviews.Services.Models.Data
{
    public class ReviewRequest : Shared.Models.BaseModel
    {
        public ReviewRequest(string partitionKey) : base(partitionKey)
        {
            this.type = ReviewType.Business;
        }

        public ReviewRequest()
        {
            this.type = ReviewType.Business;
        }

        public ReviewType type { get; set; }
        public string businessId { get; set; }

        /// <summary>
        /// User.id FK
        /// </summary>
        public long userId { get; set; }
        public long orderId { get; set; }
        
        /// <summary>
        /// Business's internal order ID
        /// </summary>
        public string businessOrderId { get; set; }

        /// <summary>
        /// BusinessCustomer ID
        /// <remarks>Not a TR user yet, just keep a reference to the original customer record.</remarks>
        /// </summary>
        public long customerId { get; set; }

        /// <summary>
        /// Only applicable for experience review (ie, ReviewType.Business)
        /// </summary>
        public string reviewId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ReviewRequestProduct> products { get; set; }
        public string businessLogoUrl { get; set; }

        public long sentOn { get; set; }

        /// <summary>
        /// Mailjet MessageID used to look up email transaction metadata
        /// </summary>
        public string emailMessageId { get; set; }

        /// <summary>
        /// Date for which this review request is scheduled to be sent
        /// </summary>
        public long sendOn { get; set; }

        public bool IsProductReviewed(long productId)
        {
            if (type == ReviewType.Business)
            {
                throw new InvalidOperationException("Not a product review request");
            }

            return this.products.Any(p => p.id == productId && p.reviewId != null);
        }
    }

    public class ReviewRequestProduct
    {
        public long id;
        public string name;
        public string imagePath;
        public string reviewId { get; set; }
    }
}
