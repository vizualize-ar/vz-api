using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Reviews.Services.Models.Request
{
    public class NewReviewRequest
    {
        /// <summary>
        /// Optional if this is a product review
        /// </summary>
        public long productId { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public decimal rating { get; set; }

        public string[] images { get; set; }
    }
}
