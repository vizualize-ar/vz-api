using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Reviews.Services.Models.Data
{
    public class ReviewVote : Shared.Models.BaseModel
    {
        public ReviewVote(string reviewId) : base(reviewId)
        {
            this.reviewId = reviewId;
        }

        public string reviewId { get; set; }
        public string userId { get; set; }
        
        /// <summary>
        /// Indicates which business website this vote came from
        /// </summary>
        public string businessId { get; set; }
        public short vote { get; set; }
    }
}
