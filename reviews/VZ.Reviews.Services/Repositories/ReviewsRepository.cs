using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using VZ.Reviews.Services.Models.Results;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;

namespace VZ.Reviews.Services.Repositories
{
    using Models.Data;

    public interface IReviewsRepository : Shared.Data.IBaseRepository<Review>
    {
        //Task<string> AddReviewsAsync(Review reviewObject);
        //Task<DeleteReviewResult> DeleteReviewAsync(string reviewId, string userId);
        //Task UpdateReviewAsync(Review reviewDocument);
        //Task<Review> GetReviewAsync(string reviewId);
        //Task<List<Review>> GetAllReviewsAsync();
        Task<Review> FindReviewWithItemAsync(string itemId, ItemType itemType, string userId);

        //Task<ReviewRequest> GetReviewRequestAsync(string id);
        //Task UpdateReviewRequestAsync(ReviewRequest request);

        // Task<List<string>> GetUnchainedReviewIds();

        // bool VerifyReviewRequestId(string id);
    }

    public class ReviewsRepository : Shared.Data.BaseRepository<Review>, IReviewsRepository
    {
        private const string ReviewCollectionName = "reviews";

        public ReviewsRepository() : base(ReviewCollectionName) { }
        
        //public async Task<string> AddReviewsAsync(Review reviewDocument)
        //{
        //    reviewDocument.NewId();
        //    reviewDocument.id += "." + reviewDocument.productId;
        //    var documentUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, ReviewCollectionName);
        //    Document doc = await DocumentClient.CreateDocumentAsync(documentUri, reviewDocument);
        //    return doc.Id;
        //}

        //public async Task<DeleteReviewResult> DeleteReviewAsync(string reviewId, string userId)
        //{
        //    var documentUri = UriFactory.CreateDocumentUri(DatabaseName, ReviewCollectionName, reviewId);
        //    try
        //    {
        //        await DocumentClient.DeleteDocumentAsync(documentUri, new RequestOptions { PartitionKey = new PartitionKey(userId) });
        //        return DeleteReviewResult.Success;
        //    }
        //    catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        //    {
        //        // we return the NotFound result to indicate the document was not found
        //        return DeleteReviewResult.NotFound;
        //    }
        //}

        //public Task UpdateReviewAsync(Review reviewDocument)
        //{
        //    var documentUri = UriFactory.CreateDocumentUri(DatabaseName, ReviewCollectionName, reviewDocument.id);
        //    var concurrencyCondition = new AccessCondition
        //    {
        //        Condition = reviewDocument.etag.ToString(),
        //        Type = AccessConditionType.IfMatch
        //    };
        //    return DocumentClient.ReplaceDocumentAsync(documentUri, reviewDocument, new RequestOptions { AccessCondition = concurrencyCondition });
        //}

        //public async Task<Review> GetReviewAsync(string reviewId)
        //{
        //    var documentUri = UriFactory.CreateDocumentUri(DatabaseName, ReviewCollectionName, reviewId);
        //    try
        //    {
        //        string productId = reviewId.Split('.')[1];
        //        var documentResponse = await DocumentClient.ReadDocumentAsync<Review>(documentUri, new RequestOptions { PartitionKey = new PartitionKey(productId) } );
        //        return documentResponse.Document;
        //    }
        //    catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        //    {
        //        // we return null to indicate the document was not found
        //        return null;
        //    }
        //}

        public async Task<Review> FindReviewWithItemAsync(string itemId, ItemType itemType, string userId)
        {
            var documentUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, ReviewCollectionName);
            
            // create a query to find the review with this item in it
            var sqlQuery = "SELECT * FROM c WHERE c.userId = @userId AND ARRAY_CONTAINS(c.items, { id: @itemId, type: @itemType }, true)";
            var sqlParameters = new SqlParameterCollection
            {
                new SqlParameter("@userId", userId),
                new SqlParameter("@itemId", itemId),
                new SqlParameter("@itemType", itemType.ToString())
            };
            var query = DocumentClient
                .CreateDocumentQuery<Review>(documentUri, new SqlQuerySpec(sqlQuery, sqlParameters))
                .AsDocumentQuery();
            
            // execute the query
            var response = await query.ExecuteNextAsync<Review>();
            return response.SingleOrDefault();
        }

        //public async Task<List<Review>> GetAllReviewsAsync()
        //{
        //    var documentUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, ReviewCollectionName);

        //    // create a query to just get the document ids
        //    var query = DocumentClient
        //        .CreateDocumentQuery<Review>(documentUri, new FeedOptions { EnableCrossPartitionQuery = true })
        //    //.Select(d => new ReviewSummary { Id = d.Id, Name = d.Name })
        //    .AsDocumentQuery();

        //    // iterate until we have all of the ids
        //    //var list = new ReviewSummaries();
        //    var list = new List<Review>();
        //    while (query.HasMoreResults)
        //    {
        //        //var summaries = await query.ExecuteNextAsync<ReviewSummary>();
        //        var summaries = await query.ExecuteNextAsync<Review>();
        //        list.AddRange(summaries.ToList());
        //    }
        //    return list;
        //}

        //public async Task<ReviewRequest> GetReviewRequestAsync(string id)
        //{
        //    if (id.Contains('.') == false) throw new ArgumentException("Invalid ID");
        //    string businessId = id.Split('.')[1];
        //    var documentUri = UriFactory.CreateDocumentUri(DatabaseName, "review_requests", id);
        //    try
        //    {
        //        var documentResponse = await DocumentClient.ReadDocumentAsync<ReviewRequest>(documentUri, new RequestOptions { PartitionKey = new PartitionKey(businessId) });
        //        return documentResponse.Document;
        //    }
        //    catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        //    {
        //        // we return null to indicate the document was not found
        //        return null;
        //    }
        //}

        //public Task UpdateReviewRequestAsync(ReviewRequest request)
        //{
        //    var documentUri = UriFactory.CreateDocumentUri(DatabaseName, "review_requests", request.id);
        //    var concurrencyCondition = new AccessCondition
        //    {
        //        Condition = request.etag,
        //        Type = AccessConditionType.IfMatch
        //    };
        //    // return DocumentClient.ReplaceDocumentAsync(documentUri, request, new RequestOptions { AccessCondition = concurrencyCondition });
        //    return DocumentClient.ReplaceDocumentAsync(documentUri, request);
        //}

        //public bool VerifyReviewRequestId(string id)
        //{
        //    if (String.IsNullOrWhiteSpace(id))
        //    {
        //        throw new ArgumentNullException("id");
        //    }

        //    if (id.Contains('.') == false)
        //    {
        //        throw new ArgumentException("Invalid ID");
        //    }

        //    try
        //    {
        //        string productId = id.Split('.')[1];
        //        var documentUri = UriFactory.CreateDocumentCollectionUri(DatabaseName, "review_requests");
        //        bool exists = DocumentClient.CreateDocumentQuery<ReviewRequest>(documentUri, new FeedOptions { PartitionKey = new PartitionKey(productId) })
        //            .Any(r => r.id == id);
        //        return exists;
        //    }
        //    catch (DocumentClientException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        //    {
        //        // we return null to indicate the document was not found
        //        return false;
        //    }
        //}
    }
}
