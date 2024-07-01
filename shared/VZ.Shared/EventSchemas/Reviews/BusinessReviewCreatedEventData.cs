using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Shared.EventSchemas.Reviews
{
    public class BusinessReviewCreatedEventData
    {
        public string reviewRequestId { get; set; }
        public string reviewId { get; set; }
        public long userId { get; set; }
        public long productId { get; set; }
    }
}
