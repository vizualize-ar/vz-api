using System;
using System.Collections.Generic;
using System.Text;
using VZ.Shared.Data;

namespace VZ.Reviews.Services.Repositories
{
    using Models.Data;

    public interface IReviewVoteRepository : IBaseRepository<ReviewVote>
    {

    }

    public class ReviewVoteRepository : BaseRepository<ReviewVote>, IReviewVoteRepository
    {
        public ReviewVoteRepository(): base("review_votes")
        {

        }
    }
}
