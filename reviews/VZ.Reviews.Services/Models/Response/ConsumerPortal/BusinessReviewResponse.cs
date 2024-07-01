using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Reviews.Services.Models.Response.ConsumerPortal
{
    public class BusinessReviewResponse
    {
        public BusinessReviewResponse()
        {
            reviews = new List<BusinessReview>();
        }

        public List<BusinessReview> reviews { get; set; }
        public string continuationToken { get; set; }

        public class BusinessReview
        {
            public BusinessReview()
            {
                replies = new List<ReviewReply>();
            }

            public string id { get; set; }
            public string title { get; set; }
            public string body { get; set; }
            public DateTime createdOn { get; set; }
            public bool verified { get; set; }
            public decimal rating { get; set; }
            public User user { get; set; }
            public List<ReviewReply> replies { get; set; }
        }

        public class User
        {
            public string name { get; set; }
            public long id { get; set; }
            public string avatarUrl { get; set; }
            public string reputation { get; set; }
        }

        public class ReviewReply
        {
            public ReviewReply(string message, long upVotes, long downVotes, bool isBusiness, DateTime createdOn)
            {
                this.message = message;
                this.upVotes = upVotes;
                this.downVotes = downVotes;
                this.isBusiness = isBusiness;
                this.createdOn = createdOn;
            }

            public string message { get; set; }
            public long upVotes { get; set; }
            public long downVotes { get; set; }
            public bool isBusiness { get; set; }
            public DateTime createdOn { get; set; }
        }
    }
}
