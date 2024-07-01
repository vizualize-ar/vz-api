using System;
using System.Collections.Generic;
using System.Text;
using VZ.Shared.Models;

namespace VZ.Reviews.Services.Models.Data
{
    public class ReviewReply : SqlBaseModel
    {
        public string reviewId { get; set; }
        public string reply { get; set; }
        public long repliedById { get; set; }
        public short upVotes { get; set; }
        public short downVotes { get; set; }
        public bool isBusiness { get; set; }
    }
}
