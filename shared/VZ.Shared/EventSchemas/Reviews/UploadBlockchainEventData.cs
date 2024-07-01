using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Shared.EventSchemas.Reviews
{
    public class UploadBlockchainEventData
    {
        public string reviewId { get; set; }
        //public string title { get; set; }
        //public string body { get; set; }

        //public string userId { get; set; }
        //public string productId { get; set; }
        //public string[] previousReviewIds { get; set; }
        public string hash { get; set; }
    }
}
