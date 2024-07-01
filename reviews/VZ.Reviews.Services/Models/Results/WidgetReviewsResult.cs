using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Reviews.Services.Models.Results
{
    public class WidgetReviewsResult
    {
        public WidgetBusinessSummary summary;
        public WidgetReview[] reviews;
    }

    public class WidgetBusinessSummary
    {
        public string slug { get; set; }
        public long totalReviews { get; set; }
        public decimal averageRating { get; set; }
    }

    public class WidgetReview
    {
        public string id { get; set; }
        public string tamperproof { get; set; }
        public bool verified { get; set; }
        public long purchasedOn { get; set; }
        public decimal rating { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public string user { get; set; }
        public string avatar { get; set; }
        public int upVotes { get; set; }
        public int downVotes { get; set; }
        public DateTime createdOn { get; set; }
        public string[] thumbnails { get; set; }
    }
}
