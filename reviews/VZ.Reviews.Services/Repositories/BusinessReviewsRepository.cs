using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VZ.Reviews.Services.Models.Results;

namespace VZ.Reviews.Services.Repositories
{
    using Dawn;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Models.Data;
    using Models.Response;

    public interface IBusinessReviewsRepository : Shared.Data.IBaseRepository<BusinessReview>
    {
        Task<Tuple<BusinessPortalReviewSummary[], string>> GetBusinessReviewsAsync(string businessId, int pageSize, string sortBy, string sortDirection, string searchText, int? rating, ReviewType? reviewType, string continuationToken);
        int GetBusinessReviewsCount(string businessId, int pageSize, string sortBy, string sortDirection, string searchText, int? rating, ReviewType? reviewType, string continuationToken);

        Task<Tuple<WidgetReview[], string>> GetWidgetBusinessReviewsAsync(ReviewType reviewType, string businessId, string externalProductId, int pageSize, int rating, string[] tags, string continuationToken);

        /// <summary>
        /// Retrieve a list of reviews that are waiting to be linked to newer reviews (those that have a tamperproof, are flagged as ready, and don't have a nextReviewId set)
        /// </summary>
        /// <returns></returns>
        Task<List<string>> GetUnchainedReviewIds();
    }

    public class BusinessReviewsRepository : Shared.Data.BaseRepository<BusinessReview>, IBusinessReviewsRepository
    {
        public BusinessReviewsRepository() : base("business_reviews")
        {

        }

        public async Task<Tuple<BusinessPortalReviewSummary[], string>> GetBusinessReviewsAsync(string businessId, int pageSize, string sortBy, string sortDirection, string searchText, int? rating, ReviewType? reviewType, string continuationToken)
        {
            //return await GetAllAsync(pageSize, continuationToken, businessId, null, br => br.createdOn, true);
            // return await GetAllAsync<BusinessReview>(pageSize, continuationToken, businessId, sort: Tuple.Create(sortBy, sortDirection));

            var query = CreateBusinessReviewsQuery(businessId, pageSize, sortBy, sortDirection, searchText, rating, reviewType, continuationToken, false);
            IDocumentQuery<dynamic> documentQuery = query.AsDocumentQuery();

            // default returns 100 (see https://docs.microsoft.com/en-us/azure/cosmos-db/sql-api-query-metrics)
            string requestContinuation = null;
            var result = await documentQuery.ExecuteNextAsync<BusinessPortalReviewSummary>();
            if (result.ResponseContinuation != null)
            {
                requestContinuation = result.ResponseContinuation;
            }
            return new Tuple<BusinessPortalReviewSummary[], string>(result.ToArray(), requestContinuation);
        }

        public int GetBusinessReviewsCount(string businessId, int pageSize, string sortBy, string sortDirection, string searchText, int? rating, ReviewType? reviewType, string continuationToken)
        {
            var query = CreateBusinessReviewsQuery(businessId, pageSize, sortBy, sortDirection, searchText, rating, reviewType, continuationToken, true);
            return query.AsEnumerable().FirstOrDefault();
        }

        private IQueryable<dynamic> CreateBusinessReviewsQuery(string businessId, int pageSize, string sortBy, string sortDirection, string searchText, int? rating, ReviewType? reviewType, string continuationToken, bool countOnly)
        {
            var documentCollectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, _collectionName);

            FeedOptions feedOptions = CreateFeedOptions(businessId, pageSize, continuationToken);

            string sql = $"select c.id, c.type, c.user.avatarUrl userAvatarUrl, c.user.name username, c.title, c.body, c.rating, c.createdOn, c.images, c.tags, c.hasReplies from c where 1=1";
            if (countOnly)
            {
                sql = $"select value count(1) from c where 1=1";
            }
            SqlParameterCollection sqlParameters = new SqlParameterCollection();
            if (string.IsNullOrWhiteSpace(searchText) == false)
            {
                // sql += " where";
                sql += " and ( CONTAINS(LOWER(c.title), @searchText)";
                sql += " or CONTAINS(LOWER(c.body), @searchText)";
                sql += " or CONTAINS(LOWER(c.user.name), @searchText)";
                sql += " or ARRAY_CONTAINS(c.tags, @searchText) )";
                sqlParameters.Add(new SqlParameter("@searchText", searchText.ToLower()));
            }
            if (rating.HasValue)
            {
                sql += " and c.rating >= @ratingMin and c.rating < @ratingMax";
                sqlParameters.Add(new SqlParameter("@ratingMin", rating.Value));
                sqlParameters.Add(new SqlParameter("@ratingMax", rating.Value + 1));
            }
            if (reviewType.HasValue)
            {
                sql += " and c.type = @reviewType";
                sqlParameters.Add(new SqlParameter("@reviewType", reviewType.Value));
            }
            if (string.IsNullOrWhiteSpace(sortBy) == false)
            {
                sql += " order by c." + sortBy;
                if (sortDirection == "desc")
                {
                    sql += " desc";
                }
            }

            SqlQuerySpec querySpec = new SqlQuerySpec(sql, sqlParameters);

            var query = DocumentClient.CreateDocumentQuery<dynamic>(documentCollectionUri, querySpec, feedOptions);
            return query;
        }

        public async Task<Tuple<WidgetReview[], string>> GetWidgetBusinessReviewsAsync(ReviewType reviewType, string businessId, string externalProductId, int pageSize, int rating, string[] tags, string continuationToken)
        {
            Guard.Argument(businessId).NotEmpty();

            string fields = @"
                c.id, c.body, c.downVotes, c.rating, c.tamperproof, c.title, c.upVotes, c.user.avatarUrl, c.user.name as user, c.verified, c.purchasedOn, c.createdOn,
                array(select value img.thumbpath from img in c.images) thumbnails";

            string from = "c";
            var whereParameters = new List<ValueTuple<string, object>>();
            string whereClause = "c.type = @type and c.status = @status and c.rating >= @rating";
            whereParameters.Add(("@type", reviewType));
            whereParameters.Add(("@status", ReviewStatus.ReadyForPublic));
            whereParameters.Add(("@rating", rating));

            if (reviewType == ReviewType.BusinessProduct)
            {
                Guard.Argument(externalProductId).NotEmpty().NotNull();

                whereClause += " and c.externalProductId = @externalProductId";
                whereParameters.Add(("@externalProductId", externalProductId));
            }
            if (tags != null && tags.Length > 0)
            {
                from = "c join tag in c.tags";
                whereClause += " and ARRAY_CONTAINS(@tags, tag)";
                whereParameters.Add(("@tags", tags));
            }
            string sql = $"select {fields} from {from} where {whereClause}";
            return await base.GetSomeRawAsync<WidgetReview>(
                businessId,
                sql,
                whereParameters.ToArray(),
                pageSize,
                continuationToken
            );
        }

        /// <summary>
        /// Retrieve a list of reviews that are waiting to be linked to newer reviews (those that have a tamperproof, are flagged as ready, and don't have a nextReviewId set)
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetUnchainedReviewIds()
        {
            var collectionLink = UriFactory.CreateDocumentCollectionUri(DatabaseName, _collectionName);
            var query = DocumentClient.CreateDocumentQuery<BusinessReview>(collectionLink, new FeedOptions { EnableCrossPartitionQuery = true })
                .Where(r => r.status == ReviewStatus.ReadyForPublic && r.nextReviewId == null)
                .Select(r => r.id)
                .AsDocumentQuery();

            List<string> reviews = new List<string>();
            while (query.HasMoreResults)
            {
                reviews.AddRange(await query.ExecuteNextAsync<string>());
            }

            return reviews;
        }
    }
}
