using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VZ.Reviews.Services.Models.Data;
using VZ.Reviews.Services.Models.Request.ConsumerPortal;
using VZ.Reviews.Services.Models.Response.ConsumerPortal;
using VZ.Reviews.Services.Repositories;
using VZ.Shared;

namespace VZ.Reviews.Services
{
    public interface ICPBusinessReviewsService
    {
        Task<List<BusinessReviewResponse>> GetBusinessReviewsAsync(string businessId);
    }

    public class CPBusinessReviewsService
    {
        protected IBusinessReviewsRepository _businessReviewsRepository;
        protected IReviewReplyRepository _reviewReplyRepository;
        private ILogger _log;

        public CPBusinessReviewsService(ILogger log) : this(log, new BusinessReviewsRepository(), new ReviewReplyRepository())
        {
        }

        public CPBusinessReviewsService(ILogger log, IBusinessReviewsRepository businessReviewsRepository, IReviewReplyRepository reviewReplyRepository)
        {
            this._log = log;
            this._businessReviewsRepository = businessReviewsRepository;
            this._reviewReplyRepository = reviewReplyRepository;
        }

        public async Task<BusinessReviewResponse> GetBusinessReviewsAsync(string businessId, string singleReviewId, int ratingFilter, SortField sortField, int take, string continuationToken)
        {
            BusinessReviewResponse response = new BusinessReviewResponse();
            List<BusinessReview> reviews = null;
            if (!String.IsNullOrWhiteSpace(singleReviewId))
            {
                reviews = new List<BusinessReview>()
                {
                    _businessReviewsRepository.Get<BusinessReview>(singleReviewId, "*", businessId)
                };
            }
            else
            {
                var reviewParams = new ValueTuple<string, object>[2];
                if (ratingFilter == 0)
                {
                    reviewParams[0] = ("@minRating", 0);
                    reviewParams[1] = ("@maxRating", 5);
                }
                else
                {
                    reviewParams[0] = ("@minRating", ratingFilter);
                    reviewParams[1] = ("@maxRating", ratingFilter + 1);
                };

                string sortExpression = "";
                switch(sortField)
                {
                    case SortField.HighestRating:
                        sortExpression = $"c.{nameof(BusinessReview.rating)} desc";
                        break;
                    case SortField.LowestRating:
                        sortExpression = $"c.{nameof(BusinessReview.rating)}";
                        break;
                    case SortField.Newest:
                        sortExpression = $"c.{nameof(BusinessReview.createdOn)} desc";
                        break;
                    case SortField.Oldest:
                        sortExpression = $"c.{nameof(BusinessReview.createdOn)}";
                        break;
                }

                var someResults = await _businessReviewsRepository.GetSomeAsync<BusinessReview>(
                    businessId, 
                    fields: "*",
                    predicate: $"c.{nameof(BusinessReview.rating)} >= @minRating and c.{nameof(BusinessReview.rating)} < @maxRating",
                    reviewParams,
                    sortExpression,
                    take,
                    continuationToken
                );
                reviews = someResults.Item1.ToList();
                response.continuationToken = someResults.Item2;
            }
            var businessReviews = reviews.Select(r => new BusinessReviewResponse.BusinessReview
            {
                id = r.id,
                body = r.body,
                rating = r.rating,
                createdOn = r.createdOn,
                title = r.title,
                verified = r.verified,
                user = new BusinessReviewResponse.User
                {
                    id = r.user.id,
                    avatarUrl = r.user.avatarUrl,
                    name = r.user.name,
                    reputation = "?"
                }
            }).ToList();

            var ids = businessReviews.Select(r => r.id).ToArray();

            var parameters = new ValueTuple<string, object>[]
            {
                ("@ids", ids)
            };
            var replies = await _reviewReplyRepository.GetSomeAsync<ReviewReply>("*", $"{nameof(ReviewReply.reviewId)} = ANY(@ids)", parameters);
            businessReviews.ForEach(review =>
            {
                review.replies = replies.Where(r => r.reviewId == review.id).Select(r => new BusinessReviewResponse.ReviewReply(r.reply, r.upVotes, r.downVotes, r.isBusiness, r.createdOn)).ToList();
            });

            response.reviews = businessReviews;
            return response;
        }
    }
}
