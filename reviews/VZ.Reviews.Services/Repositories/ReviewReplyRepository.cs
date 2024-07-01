using System;
using System.Collections.Generic;
using System.Text;
using VZ.Shared.Data;
using VZ.Reviews.Services.Models.Data;

namespace VZ.Reviews.Services.Repositories
{
    public interface IReviewReplyRepository : ISqlBaseRepository<ReviewReply>
    {

    }

    public class ReviewReplyRepository : SqlBaseRepository<ReviewReply>, IReviewReplyRepository
    {
        public ReviewReplyRepository() : base("review_replies")
        {

        }
    }
}
