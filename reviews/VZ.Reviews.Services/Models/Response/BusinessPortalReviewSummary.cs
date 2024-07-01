using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Reviews.Services.Models.Response
{
    public class BusinessPortalReviewSummary
    {
        public BusinessPortalReviewSummary()
        {
            images = new Data.ReviewImageList();
        }

        public string id { get; set; }
        public int type { get; set; }
        public string userAvatarUrl { get; set; }
        public string username { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public decimal rating { get; set; }
        public DateTime createdOn { get; set; }
        public string[] thumbnails { get; set; }

        public Models.Data.ReviewImageList images { get; set; }
        public string[] tags { get; set; }
    }
}
