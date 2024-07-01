using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrueRevue.Reviews.Services.Models;
using TrueRevue.Reviews.Services.Models.Data;
using TrueRevue.Reviews.Services.Models.Response;
using TrueRevue.Reviews.Services.Models.Results;
using TrueRevue.Reviews.Services.Repositories;

namespace TrueRevue.Reviews.Services.Tests
{
    public class FakeReviewsRepository : IReviewsRepository
    {
        public readonly IList<Review> ReviewDocuments = new List<Review>();


     
        public Task<string> AddReviewsAsync(Review reviewDocument)
        {
            if (string.IsNullOrEmpty(reviewDocument.Id))
            {
                reviewDocument.Id = Guid.NewGuid().ToString();
            }

            ReviewDocuments.Add(reviewDocument);
            return Task.FromResult(reviewDocument.Id);
        }

        public Task<DeleteReviewResult> DeleteReviewAsync(string reviewId, string userId)
        {
            var documentToRemove = ReviewDocuments.SingleOrDefault(d => d.Id == reviewId);
            if (documentToRemove == null)
            {
                return Task.FromResult(DeleteReviewResult.NotFound);
            }

            ReviewDocuments.Remove(documentToRemove);
            return Task.FromResult(DeleteReviewResult.Success);
        }

        public Task UpdateReviewAsync(Review reviewDocument)
        {
            var documentToUpdate = ReviewDocuments.SingleOrDefault(d => d.Id == reviewDocument.Id);
            if (documentToUpdate == null)
            {
                throw new InvalidOperationException("UpdateTextAsync called for document that does not exist.");
            }
            documentToUpdate.name = reviewDocument.name;
            return Task.CompletedTask;
        }

        public Task<Review> GetReviewAsync(string reviewId, string userId)
        {
            var document = ReviewDocuments.SingleOrDefault(d => d.Id == reviewId);
            return Task.FromResult(document);
        }

        public Task<List<Review>> GetAllReviewsAsync()
        {
            var list = ReviewDocuments
                //.Where(d => d.UserId == userId)
                //.Select(d => new ReviewSummary { Id = d.Id, Name = d.name})
                .ToList();
            //var reviewSummaries = new ReviewSummaries();
            //reviewSummaries.AddRange(list);
            //return Task.FromResult(reviewSummaries);
            return Task.FromResult(list);
        }

        public Task<Review> FindReviewWithItemAsync(string itemId, ItemType itemType, string userId)
        {
            var list = ReviewDocuments
                //.Where(d => d.UserId == userId && d.Items.Any(i => i.Id == itemId && i.Type == itemType))
                .ToList();
            return Task.FromResult(list.SingleOrDefault());
        }

    }
}
