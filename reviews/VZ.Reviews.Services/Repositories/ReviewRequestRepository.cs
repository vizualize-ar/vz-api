using System;
using System.Collections.Generic;
using System.Text;
using VZ.Shared.Data;

namespace VZ.Reviews.Services.Repositories
{
    using Models.Data;

    public interface IReviewRequestRepository : IBaseRepository<ReviewRequest>
    {

    }

    public class ReviewRequestRepository : BaseRepository<ReviewRequest>, IReviewRequestRepository
    {
        public ReviewRequestRepository(): base("review_requests")
        {

        }
    }
}
