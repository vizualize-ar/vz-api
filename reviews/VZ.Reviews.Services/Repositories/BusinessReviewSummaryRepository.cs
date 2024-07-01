using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Reviews.Services.Repositories
{
    public interface IBusinessReviewSummaryRepository : Shared.Data.IBaseRepository<Models.Data.BusinessReviewSummary>
    {
    }

    public class BusinessReviewSummaryRepository : Shared.Data.BaseRepository<Models.Data.BusinessReviewSummary>, IBusinessReviewSummaryRepository
    {
        public BusinessReviewSummaryRepository() : base("business_review_summaries")
        {

        }
    }
}
