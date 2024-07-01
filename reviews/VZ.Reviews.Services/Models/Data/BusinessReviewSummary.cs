using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Reviews.Services.Models.Data
{
    public class BusinessReviewSummary : Shared.Models.BaseModel
    {
        public string businessId { get; set; }
        public ReviewType type { get; set; }
        public string slug { get; set; }
        public long totalReviews { get; set; }
        public decimal averageRating { get; set; }
    }
}
