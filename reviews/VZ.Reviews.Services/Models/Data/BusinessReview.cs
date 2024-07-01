using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Reviews.Services.Models.Data
{
    public class BusinessReview : Review
    {
        public string businessId { get; set; }

        public long businessProductId { get; set; }

        /// <summary>
        /// Businesses's product ID
        /// </summary>
        public string externalProductId { get; set; }

        public ReviewType type { get; set; }
    }
}
