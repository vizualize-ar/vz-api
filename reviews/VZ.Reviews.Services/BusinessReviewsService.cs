using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VZ.Reviews.Services.Models;
using VZ.Reviews.Services.Models.Response;
using VZ.Reviews.Services.Models.Results;
using VZ.Reviews.Services.Repositories;
using VZ.Shared;
using VZ.Shared.EventSchemas.Audio;
using VZ.Shared.EventSchemas.Reviews;
using VZ.Shared.EventSchemas.Images;
using VZ.Shared.EventSchemas.Text;
using Microsoft.Extensions.Logging;

namespace VZ.Reviews.Services
{
    using Models.Data;
    using Shared.Data;
    using Models.Response.ConsumerPortal;
    using System.Diagnostics;

    public interface IBusinessReviewsService
    {
        Task AddImagesToReview(string reviewId, string[] imageUrls);

        Task AddImageThumbnailToReview(string reviewId, string thumbnailPath);

        // Task<string> AddReviewAsync(NewReviewRequest review);
        Task AddReviewForRequestAsync(Services.Models.Request.NewReviewRequest review, ReviewRequest request);
        Task<string> AddReviewRequestAsync(ReviewRequest reviewRequest);

        Task<DeleteDocumentResult> DeleteReviewAsync(string reviewId, string userId);
        Task<List<Review>> GetAllReviewsAsync();

        Task<BusinessReview> GetBusinessReviewAsync(string reviewId);

        // Task<Tuple<BusinessReview[], string>> GetBusinessReviewsAsync(ReviewType reviewType, string businessId, string productId, int pageSize, string continuationToken);
        Task<Tuple<WidgetReview[], string>> GetWidgetBusinessReviewsAsync(ReviewType reviewType, string businessId, string externalProductId, int pageSize, int rating, string[] tags, string continuationToken);

        Task<BusinessReviewSummary> GetBusinessReviewSummary(string businessId, string externalProductId = null);

        Task<ReviewRequest> GetExperienceReviewRequestAsync(string businessId, string businessOrderId);

        Task<ReviewRequest> GetProductReviewRequestsAsync(string businessId, long orderId, DateTime sendOn);

        //Task<UpdateReviewResult> UpdateReviewAsync(string reviewId, string userId, string name);
        Task<Review> GetReviewAsync(string reviewId);
        Task<ReviewRequest> GetReviewRequestAsync(string id);

        ReviewRequestResponse GetReviewRequestResponse(string id, ReviewType reviewType);

        Task ProcessAddItemEventAsync(EventGridEvent eventToProcess);

        Task ProcessBlockchainUploadCompleteEvent(EventGridEvent<BlockchainUploadCompleteEventData> eventToProcess);

        Task ProcessDeleteItemEventAsync(EventGridEvent eventToProcess, string userId);

        //Task ProcessReviewUpdatedEventAsync(EventGridEvent<UpdateReviewUserInfoEventData> eventToProcess);

        Task ProcessUpdateItemEventAsync(EventGridEvent eventToProcess, string userId);

        Task UpdateBusinessReviewAsync(BusinessReview review);

        Task UpdateBusinessReviewSummaryAsync(BusinessReviewSummary summary);

        Task UpdateBusinessReviewVote(string reviewId, string userId, short vote);

        Task UpdateReviewAsync(Review review);

        Task UpdateReviewRequestAsync(ReviewRequest reviewRequest);
    }

    public class BusinessReviewsService : IBusinessReviewsService
    {
        protected IBusinessReviewsRepository _businessReviewsRepository;
        protected IBusinessReviewSummaryRepository _businessReviewSummaryRepository;
        protected IEventGridPublisherService _eventGridPublisher;
        // protected IReviewsRepository _reviewsRepository;
        private ILogger _log;
        private IReviewRequestRepository _reviewRequestRepository;

        public BusinessReviewsService(ILogger log) : this(log, new EventGridPublisherService(log), new ReviewsRepository(), new BusinessReviewSummaryRepository(), new BusinessReviewsRepository(), new ReviewRequestRepository())
        {
        }

        public BusinessReviewsService(ILogger log, IEventGridPublisherService eventGridPublisher, IReviewsRepository reviewsRepository, IBusinessReviewSummaryRepository summaryRepo, IBusinessReviewsRepository businessReviewsRepo, IReviewRequestRepository reviewRequestRepository)
        {
            _log = log;
            _eventGridPublisher = eventGridPublisher;
            // _reviewsRepository = reviewsRepository;
            _businessReviewSummaryRepository = summaryRepo;
            _businessReviewsRepository = businessReviewsRepo;
            _reviewRequestRepository = reviewRequestRepository;
        }

        public async Task<string[]> GetBusinessReviewTagsAsync(string businessId)
        {
            var parameters = new ValueTuple<string, object>[]
            {
                ("@businessId", businessId)
            };
            var result = await _businessReviewsRepository.GetSomeAsync<string[]>(businessId, fields: "VALUE c.tags", "c.businessId = @businessId", parameters);
            return result.Item1.SelectMany(t => t.ToArray()).Select(s => s.ToLower()).Distinct().OrderBy(s => s).ToArray();
        }

        //public async Task<string> AddReviewAsync(NewReviewRequest review)
        //{
        //    var newReview = new Review();
        //    newReview.createdOn = DateTime.UtcNow;
        //    if (!ValidateNewReview(newReview, out var errors))
        //    {
        //        throw new ValidationListException(errors);
        //    }
        //    var reviewId = await _reviewsRepository.AddAsync(newReview);

        //    //// post a ReviewCreated event to Event Grid
        //    //var eventData = new ReviewCreatedEventData
        //    //{
        //    //    Name = name
        //    //};
        //    //var subject = $"{userId}/{reviewId}";
        //    //await EventGridPublisher.PostEventGridEventAsync(EventTypes.Reviews.ReviewCreated, subject, eventData);

        //    return reviewId;
        //}

        //public async Task AddReviewForRequestAsync(Services.Models.Request.NewReviewRequest userReviews, ReviewRequest request)
        //{
        //    if (request.type == ReviewType.Business)
        //    {
        //        if (request.reviewId != null)
        //        {
        //            // Experience review already created
        //            return;
        //        }
        //    }

        //    #region Validation

        //    List<ValidationResult> allValidationErrors = new List<ValidationResult>();
        //    List<BusinessReview> validReviews = new List<BusinessReview>();
        //    foreach (var userReview in userReviews.reviews)
        //    {
        //        if (request.type == ReviewType.BusinessProduct)
        //        {
        //            var reviewCreated = request.products.Any(p => p.id == userReview.productId && p.reviewId != null);
        //            if (reviewCreated)
        //            {
        //                // Product review already created
        //                continue;
        //            }
        //        }
        //        var newReview = new BusinessReview();
        //        newReview.createdOn = DateTime.UtcNow;
        //        newReview.body = userReview.body;
        //        newReview.businessId = request.businessId;
        //        newReview.title = userReview.title;
        //        newReview.type = request.type;
        //        newReview.user = new ReviewUser();
        //        newReview.user.id = request.customerId;
        //        newReview.status = ReviewStatus.New;

        //        if (!ValidateNewReview(newReview, out var validationErrors))
        //        {
        //            allValidationErrors.AddRange(validationErrors);
        //            continue;
        //        }

        //        if (newReview.type == ReviewType.BusinessProduct)
        //        {
        //            // TODO: Create Product class and figure out how to link it
        //            newReview.productId = null;
        //            newReview.businessProductId = userReview.productId;
        //            newReview.NewId(newReview.businessProductId);
        //        }
        //        else if (newReview.type == ReviewType.Business)
        //        {
        //            newReview.NewId(newReview.businessId);
        //        }
        //        validReviews.Add(newReview);
        //    }

        //    #endregion Validation

        //    #region Persistence

        //    foreach (var newReview in validReviews)
        //    {
        //        await _reviewsRepository.AddAsync(newReview);
        //        if (newReview.type == ReviewType.Business)
        //        {
        //            request.reviewId = newReview.id;
        //            await _reviewRequestRepository.UpdateAsync(request);
        //        }
        //        else if (newReview.type == ReviewType.BusinessProduct)
        //        {
        //            var product = request.products.First(p => p.id == newReview.businessProductId);
        //            product.reviewId = newReview.id;
        //        }                

        //        // post a ReviewCreated event to Event Grid
        //        var eventData = new UpdateReviewUserInfoEventData
        //        {
        //            reviewId = newReview.id,
        //            userId = newReview.user.id
        //        };

        //        var subject = $"{newReview.id}/{request.customerId}/{newReview.productId}";
        //        await _eventGridPublisher.PostEventGridEventAsync(EventTypes.Reviews.ReviewCreated, subject, eventData);

        //        await AddToChain(newReview.id);
        //    }

        //    #endregion Persistence

        //    // return validation results
        //    if (allValidationErrors.Count > 0)
        //    {
        //        throw new ValidationListException(allValidationErrors);
        //    }
        //}

        public async Task AddImagesToReview(string reviewId, string[] imageUrls)
        {
            var result = UpdateDocumentResult.Success;
            do
            {
                var review = await GetBusinessReviewAsync(reviewId);
                bool updated = false;
                foreach (var imageUrl in imageUrls)
                {
                    var reviewImage = new ReviewImage(imageUrl);
                    if (review.images.Contains(reviewImage) == false)
                    {
                        review.images.Add(reviewImage);
                        updated = true;
                    }
                }
                if (updated)
                {
                    result = await _businessReviewsRepository.UpdateAsync(review);
                }
            } while (result == UpdateDocumentResult.Failed_Stale);
        }

        public async Task AddImageThumbnailToReview(string reviewId, string thumbnailPath)
        {
            var result = UpdateDocumentResult.Success;
            do
            {
                var review = await GetBusinessReviewAsync(reviewId);
                string thumbName = System.IO.Path.GetFileName(thumbnailPath);
                // full and thumb images will have the same name, just in different folders
                var reviewImage = review.images.First(ri => ri.name == thumbName);
                reviewImage.thumbpath = thumbnailPath;
                result = await _businessReviewsRepository.UpdateAsync(review);
            } while (result == UpdateDocumentResult.Failed_Stale);
        }

        public async Task AddReviewForRequestAsync(Services.Models.Request.NewReviewRequest userReview, ReviewRequest request)
        {
            if (request.type == ReviewType.Business)
            {
                if (request.reviewId != null)
                {
                    // Experience review already created
                    return;
                }
            }

            #region Validation

            if (request.type == ReviewType.BusinessProduct)
            {
                var reviewCreated = request.products.Any(p => p.id == userReview.productId && p.reviewId != null);
                if (reviewCreated)
                {
                    // Product review already created
                    //continue;
                    return;
                }
            }
            var newReview = new BusinessReview();
            newReview.createdOn = DateTime.UtcNow;
            newReview.body = userReview.body;
            newReview.businessId = request.businessId;
            newReview.rating = userReview.rating;
            newReview.title = userReview.title;
            newReview.type = request.type;
            newReview.user = new ReviewUser();
            newReview.user.id = request.userId;
            newReview.user.businessCustomerId = request.customerId;
            newReview.verified = true;
            newReview.status = ReviewStatus.New;

            if (newReview.type == ReviewType.BusinessProduct)
            {
                // TODO: Create Product class and figure out how to link it
                newReview.productId = null;
                newReview.businessProductId = userReview.productId;
            }

            if (!ValidateNewReview(newReview, out var validationErrors))
            {

                //allValidationErrors.AddRange(validationErrors);
                //continue;
                throw new ValidationListException(validationErrors);
            }

            if (newReview.type == ReviewType.BusinessProduct)
            {
                newReview.NewId(newReview.businessProductId.ToString(), newReview.businessId);
            }
            else if (newReview.type == ReviewType.Business)
            {
                newReview.NewId(newReview.businessId);
            }

            #endregion Validation

            #region Persistence

            await _businessReviewsRepository.AddAsync(newReview);
            if (newReview.type == ReviewType.Business)
            {
                request.reviewId = newReview.id;
                await _reviewRequestRepository.UpdateAsync(request);
            }
            else if (newReview.type == ReviewType.BusinessProduct)
            {
                var product = request.products.First(p => p.id == newReview.businessProductId);
                product.reviewId = newReview.id;
                await _reviewRequestRepository.UpdateAsync(request);
            }

            // post a ReviewCreated event to Event Grid
            var eventData = new BusinessReviewCreatedEventData
            {
                reviewRequestId = request.id,
                reviewId = newReview.id,
                userId = newReview.user.id,
                productId = userReview.productId
            };

            var subject = $"/review_created/{request.customerId}";
            await _eventGridPublisher.PostEventGridEventAsync(EventTypes.Reviews.BusinessReviewCreated, subject, eventData);

            await QueueBusinessReviewForBlockchain(newReview.id);

            #endregion Persistence
        }

        public async Task<string> AddReviewRequestAsync(ReviewRequest reviewRequest)
        {
            if (String.IsNullOrWhiteSpace(reviewRequest.id))
            {
                reviewRequest.NewId(reviewRequest.businessId);
            }
            return await _reviewRequestRepository.AddAsync(reviewRequest);
        }
        /// <summary>
        /// Adds a review to the ledger we keep using the blockchain hashes. This assumes the review has a tamperproof ID (blockchain address)
        /// </summary>
        /// <param name="review"></param>
        /// <returns></returns>
        public async Task QueueBusinessReviewForBlockchain(string reviewId)
        {
            //var eventData = new UploadBlockchainEventData
            //{
            //    reviewId = review.id,
            //    title = review.title,
            //    body = review.body,
            //    userId = review.user.id,
            //    productId = review.productId,
            //    previousReviewIds = new string[] { "123", "456" }
            //};
            //var subject = $"reviews.blockchainupload";
            //await EventGridPublisher.PostEventGridEventAsync(EventTypes.Reviews.UploadBlockchain, subject, eventData);
            //return;

            var review = await _businessReviewsRepository.GetAsync(reviewId, reviewId.PartitionKeyPart());
            List<string> previousReviewIds = await _businessReviewsRepository.GetUnchainedReviewIds();
            review.previousReviewIds = new List<string>();
            review.previousReviewIds.AddRange(previousReviewIds);

            await _businessReviewsRepository.UpdateAsync(review);

            var eventData = new UploadBlockchainEventData
            {
                reviewId = review.id,
                //title = review.title,
                //body = review.body,
                //userId = review.user.id,
                //productId = review.productId,
                //previousReviewIds = review.previousReviewIds.ToArray()
                hash = review.GetHash()
            };

            var subject = $"reviews.blockchainupload";
            await _eventGridPublisher.PostEventGridEventAsync(EventTypes.Reviews.UploadBlockchain, subject, eventData);
        }

        public async Task<DeleteDocumentResult> DeleteReviewAsync(string reviewId, string userId)
        {
            //// delete the document from Cosmos DB
            //var result = await _reviewsRepository.DeleteAsync(reviewId, userId);
            //if (result == DeleteDocumentResult.NotFound)
            //{
            //    return DeleteDocumentResult.NotFound;
            //}

            //// post a ReviewDeleted event to Event Grid
            //var subject = $"{userId}/{reviewId}";
            //await _eventGridPublisher.PostEventGridEventAsync(EventTypes.Reviews.ReviewDeleted, subject, new ReviewDeletedEventData());

            //return DeleteDocumentResult.Success;

            throw new NotImplementedException();
        }

        //public async Task<UpdateReviewResult> UpdateReviewAsync(string reviewId, string userId, string name)
        //{
        //    // get the current version of the document from Cosmos DB
        //    var reviewDocument = await ReviewsRepository.GetReviewAsync(reviewId, userId);
        //    if (reviewDocument == null)
        //    {
        //        return UpdateReviewResult.NotFound;
        //    }

        //    // update the document with the new name
        //    reviewDocument.name = name;
        //    await ReviewsRepository.UpdateReviewAsync(reviewDocument);

        //    // post a ReviewNameUpdated event to Event Grid
        //    var eventData = new ReviewNameUpdatedEventData
        //    {
        //        Name = name
        //    };
        //    var subject = $"{userId}/{reviewId}";
        //    await EventGridPublisher.PostEventGridEventAsync(EventTypes.Reviews.ReviewNameUpdated, subject, eventData);

        //    return UpdateReviewResult.Success;
        //}

        public async Task<List<Review>> GetAllReviewsAsync()
        {
            //var result = await _reviewsRepository.GetAllAsync();
            //return result;
            throw new NotImplementedException();
        }

        public async Task<BusinessReview> GetBusinessReviewAsync(string reviewId)
        {
            return await _businessReviewsRepository.GetAsync(reviewId, reviewId.PartitionKeyPart());
        }

        //public async Task<Tuple<BusinessReview[], string>> GetBusinessReviewsAsync(string businessId, int pageSize, string sortBy, string sortDirection, string searchText, int? rating, string continuationToken)
        public (BusinessPortalReviewSummary[] reviews, string continuationToken, int count) GetBusinessReviewsAsync(string businessId, int pageSize, string sortBy, string sortDirection, string searchText, int? rating, bool experienceReviews, bool productReviews, string continuationToken)
        {
            Task<Tuple<BusinessPortalReviewSummary[], string>> listTask;
            int count;
            if ((experienceReviews && productReviews) || (!experienceReviews && !productReviews))
            {
                listTask = _businessReviewsRepository.GetBusinessReviewsAsync(businessId, pageSize, sortBy, sortDirection, searchText, rating, null, continuationToken);
                count = _businessReviewsRepository.GetBusinessReviewsCount(businessId, pageSize, sortBy, sortDirection, searchText, rating, null, continuationToken);
            }
            else if (productReviews)
            {
                listTask = _businessReviewsRepository.GetBusinessReviewsAsync(businessId, pageSize, sortBy, sortDirection, searchText, rating, ReviewType.BusinessProduct, continuationToken);
                count = _businessReviewsRepository.GetBusinessReviewsCount(businessId, pageSize, sortBy, sortDirection, searchText, rating, ReviewType.BusinessProduct, continuationToken);
            }
            else
            {
                listTask = _businessReviewsRepository.GetBusinessReviewsAsync(businessId, pageSize, sortBy, sortDirection, searchText, rating, ReviewType.Business, continuationToken);
                count = _businessReviewsRepository.GetBusinessReviewsCount(businessId, pageSize, sortBy, sortDirection, searchText, rating, ReviewType.Business, continuationToken);
            }

            Task.WaitAll(listTask);
            var listResult = listTask.Result;

            return (listResult.Item1, listResult.Item2, count);
        }

        public async Task<Tuple<WidgetReview[], string>> GetWidgetBusinessReviewsAsync(ReviewType reviewType, string businessId, string externalProductId, int pageSize, int rating, string[] tags, string continuationToken)
        {
            var result = await _businessReviewsRepository.GetWidgetBusinessReviewsAsync(reviewType, businessId, externalProductId, pageSize, rating, tags, continuationToken);
            return result;
        }

        /// <summary>
        /// Get the review summary for a business or a product.
        /// </summary>
        /// <param name="businessId"></param>
        /// <param name="externalProductId"></param>
        /// <returns></returns>
        public async Task<BusinessReviewSummary> GetBusinessReviewSummary(string businessId, string externalProductId = null)
        {
            BusinessReviewSummary summary;
            string id = businessId;
            if (externalProductId != null)
            {
                id = string.Format("{0}{1}{2}", externalProductId, "".IdDelimiter(), businessId);
            }

            summary = await _businessReviewSummaryRepository.GetAsync(id, businessId);
            if (summary == null)
            {
                summary = new BusinessReviewSummary()
                {
                    id = id,
                    businessId = businessId,
                    type = externalProductId == null ? ReviewType.Business : ReviewType.BusinessProduct
                };
                await _businessReviewSummaryRepository.AddAsync(summary);
            }
            return summary;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="businessId"></param>
        /// <param name="businessOrderId">BusinessOrder.orderId</param>
        /// <returns></returns>
        public async Task<ReviewRequest> GetExperienceReviewRequestAsync(string businessId, string businessOrderId)
        {
            var result = await _reviewRequestRepository.GetAllAsync(businessId, predicate: rr => rr.businessOrderId == businessOrderId && rr.type == ReviewType.Business);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="businessId"></param>
        /// <param name="orderId">BusinessOrder.id</param>
        /// <returns></returns>
        public async Task<ReviewRequest> GetProductReviewRequestsAsync(string businessId, long orderId, DateTime sendOn)
        {
            long sendOnTimestamp = new DateTimeOffset(sendOn).ToUnixTimeSeconds();
            var result = await _reviewRequestRepository.GetAllAsync(businessId, predicate: rr => rr.orderId == orderId && rr.sendOn == sendOnTimestamp && rr.type == ReviewType.BusinessProduct);
            return result.FirstOrDefault();
        }

        public async Task<Review> GetReviewAsync(string reviewId)
        {
            //var review = await _reviewsRepository.GetAsync(reviewId);
            //if (review == null)
            //{
            //    return null;
            //}

            //return review;

            throw new NotImplementedException();
        }

        public async Task<List<Shared.Models.DateCountResult>> GetBusinessReviewCountByDate(string businessId, DateTime from, DateTime to)
        {
            string key = businessId + from.ToFileTimeUtc().ToString() + to.ToFileTimeUtc().ToString();
            var result = CacheManager.Get<List<Shared.Models.DateCountResult>>(key);

            if (result == null)
            {
                var counts = await _businessReviewsRepository.GetSomeAsync<dynamic>(
                    partitionKey: businessId,
                    select: r => new { r.createdOn },
                    predicate: r => r.createdOn > from && r.createdOn < to,
                    orderBy: r => r.createdOn, false
                );

                // var list = counts.Item1.Select(s => new { date = DateTimeOffset.FromUnixTimeSeconds(s.createdOn).Date });
                result = new List<Shared.Models.DateCountResult>();
                for (var currentDate = from; currentDate <= to; currentDate = currentDate.AddDays(1))
                {
                    //result.Add(new Shared.Models.DateCountResult(currentDate, list.Where(c => c.date == currentDate).Count()));
                    result.Add(new Shared.Models.DateCountResult(currentDate, counts.Item1.Where(c => c.createdOn.Date == currentDate).Count()));
                }
                CacheManager.Set(key, result, TimeSpan.FromDays(1));
            }

            return result;
        }

        public async Task<ReviewRequest> GetReviewRequestAsync(string id)
        {
            return await _reviewRequestRepository.GetAsync(id, id.PartitionKeyPart());
        }

        public async Task<List<ReviewRequest>> GetReviewRequestsForOrdersAsync(long[] orderIds)
        {
            if (orderIds == null || orderIds.Length == 0) return new List<ReviewRequest>();

            //var businessId = orderIds[0].PartitionKeyPart();
            var businessId = (string)(await ApiHelper.Instance.GetOrderAsync(orderIds[0])).businessId;
            return await _reviewRequestRepository.GetAllAsync(businessId, rr => orderIds.Contains(rr.orderId));
        }

        public ReviewRequestResponse GetReviewRequestResponse(string id, ReviewType reviewType)
        {
            if (reviewType == ReviewType.BusinessProduct)
            {
                var reviewRequest = _reviewRequestRepository.Get(id, "c.id, c.businessLogoUrl, c.reviewId, c.products, c.type", id.PartitionKeyPart());
                return new ReviewRequestResponse
                {
                    businessLogoUrl = reviewRequest.businessLogoUrl,
                    id = reviewRequest.id,
                    reviewId = reviewRequest.reviewId,
                    products = ((JArray)reviewRequest.products).ToObject<List<ReviewRequestProductResponse>>(),
                    type = (ReviewType)Enum.Parse(typeof(ReviewType), reviewRequest.type.ToString())
                };
            }
            else if (reviewType == ReviewType.Business)
            {
                var reviewRequest = _reviewRequestRepository.Get(id, "c.id, c.businessLogoUrl, c.reviewId, c.type", id.PartitionKeyPart());
                return new ReviewRequestResponse
                {
                    businessLogoUrl = reviewRequest.businessLogoUrl,
                    id = reviewRequest.id,
                    reviewId = reviewRequest.reviewId,
                    type = (ReviewType)Enum.Parse(typeof(ReviewType), reviewRequest.type.ToString())
                };
            }

            throw new InvalidOperationException($"Specified ReviewType not handled. ID: {id}, ReviewType: {reviewType}");
        }

        public async Task ProcessAddItemEventAsync(EventGridEvent eventToProcess)
        {
            //// process the item type
            //var (item, reviewId, operationType) = ConvertEventGridEventToReviewItem(eventToProcess);
            //if (operationType != OperationType.Add)
            //{
            //    return;
            //}

            //// find the review document
            //var reviewDocument = await _reviewsRepository.GetAsync(reviewId);
            //if (reviewDocument == null)
            //{
            //    return;
            //}

            ////// update the document with the new item
            ////// and if the item already exists in this review, replace it
            ////var existingItem = reviewDocument.Items.SingleOrDefault(i => i.Id == item.Id);
            ////if (existingItem != null)
            ////{
            ////    reviewDocument.Items.Remove(existingItem);
            ////}
            ////reviewDocument.Items.Add(item);
            //await _reviewsRepository.UpdateAsync(reviewDocument);

            //// post a ReviewItemsUpdated event to Event Grid
            //var eventData = new ReviewItemsUpdatedEventData();
            ////var subject = $"{userId}/{reviewDocument.id}";
            //var subject = $"{reviewDocument.id}";
            //await _eventGridPublisher.PostEventGridEventAsync(EventTypes.Reviews.ReviewItemsUpdated, subject, eventData);

            throw new NotImplementedException();
        }

        public async Task ProcessBlockchainUploadCompleteEvent(EventGridEvent<BlockchainUploadCompleteEventData> eventToProcess)
        {
            var reviewData = eventToProcess.Data;
            var review = await _businessReviewsRepository.GetAsync(reviewData.review.reviewId, reviewData.review.reviewId.PartitionKeyPart());
            review.tamperproof = reviewData.transaction;
            review.tamperproofedOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            review.status = ReviewStatus.ReadyForPublic;
            await _businessReviewsRepository.UpdateAsync(review);
        }

        public async Task ProcessDeleteItemEventAsync(EventGridEvent eventToProcess, string userId)
        {
            //// process the item type
            //var (updatedItem, _, operationType) = ConvertEventGridEventToReviewItem(eventToProcess);
            //if (operationType != OperationType.Delete)
            //{
            //    return;
            //}

            //// find the review document
            //var reviewDocument = await _reviewsRepository.FindReviewWithItemAsync(updatedItem.Id, updatedItem.Type, userId);
            //if (reviewDocument == null)
            //{
            //    return;
            //}

            ////// find the item in the document
            ////var itemToRemove = reviewDocument.Items.SingleOrDefault(i => i.Id == updatedItem.Id);
            ////if (itemToRemove == null)
            ////{
            ////    return;
            ////}

            ////// remove the item from the document
            ////reviewDocument.Items.Remove(itemToRemove);
            //await _reviewsRepository.UpdateAsync(reviewDocument);

            //// post a ReviewItemsUpdated event to Event Grid
            //var eventData = new ReviewItemsUpdatedEventData();
            //var subject = $"{userId}/{reviewDocument.id}";
            //await _eventGridPublisher.PostEventGridEventAsync(EventTypes.Reviews.ReviewItemsUpdated, subject, eventData);

            throw new NotImplementedException();
        }

        //public async Task ProcessReviewUpdatedEventAsync(EventGridEvent<UpdateReviewUserInfoEventData> eventToProcess)
        //{
        //    var reviewData = eventToProcess.Data;
        //    var review = await _reviewsRepository.GetAsync(reviewData.reviewId);
        //    review.user.name = reviewData.username;
        //    review.user.avatarUrl = reviewData.userAvatar;
        //    await _reviewsRepository.UpdateAsync(review);
        //}

        public async Task ProcessUpdateItemEventAsync(EventGridEvent eventToProcess, string userId)
        {
            //// process the item type
            //var (updatedItem, _, operationType) = ConvertEventGridEventToReviewItem(eventToProcess);
            //if (operationType != OperationType.Update)
            //{
            //    return;
            //}

            //// find the review document
            //var reviewDocument = await _reviewsRepository.FindReviewWithItemAsync(updatedItem.Id, updatedItem.Type, userId);
            //if (reviewDocument == null)
            //{
            //    return;
            //}

            ////// find the item in the document
            ////var existingItem = reviewDocument.Items.SingleOrDefault(i => i.Id == updatedItem.Id);
            ////if (existingItem == null)
            ////{
            ////    return;
            ////}
            ////// update the item with the latest changes
            ////// (the only field that can change is Preview)
            ////existingItem.Preview = updatedItem.Preview;

            //await _reviewsRepository.UpdateAsync(reviewDocument);

            //// post a reviewItemsUpdated event to Event Grid
            //var eventData = new ReviewItemsUpdatedEventData();
            //var subject = $"{userId}/{reviewDocument.id}";
            //await _eventGridPublisher.PostEventGridEventAsync(EventTypes.Reviews.ReviewItemsUpdated, subject, eventData);

            throw new NotImplementedException();
        }

        public async Task UpdateBusinessReviewAsync(BusinessReview review)
        {
            await _businessReviewsRepository.UpdateAsync(review);
        }

        public async Task UpdateBusinessReviewSummaryAsync(BusinessReviewSummary summary)
        {
            await _businessReviewSummaryRepository.UpdateAsync(summary);
        }

        public async Task UpdateBusinessReviewVote(string reviewId, string userId, short vote)
        {
            var reviewVoteRepository = new ReviewVoteRepository();

            var review = await _businessReviewsRepository.GetAsync(reviewId, reviewId.PartitionKeyPart());
            var reviewVote = reviewVoteRepository.GetAll(reviewId, "c.userId = @userId", ("@userId", userId)).FirstOrDefault();

            Task reviewUpdateTask = Task.CompletedTask, reviewVoteUpdateTask = Task.CompletedTask;
            if (reviewVote == null)
            {
                if (vote == 0) return;

                if (vote > 0)
                {
                    review.upVotes++;
                }
                else if (vote < 0)
                {
                    review.downVotes++;
                }
                reviewUpdateTask = _businessReviewsRepository.UpdateAsync(review);

                reviewVote = new ReviewVote(reviewId)
                {
                    businessId = review.businessId,
                    userId = userId,
                    vote = vote
                };
                reviewVoteUpdateTask = reviewVoteRepository.AddAsync(reviewVote);
            }
            else
            {
                if (reviewVote.vote != vote)
                {
                    if (reviewVote.vote < 0)
                    {
                        // was previously a downvote
                        review.downVotes--;
                    }
                    else if (reviewVote.vote > 0)
                    {
                        // was previously an upvote
                        review.upVotes--;
                    }

                    if (vote > 0)
                    {
                        // new vote is an upvote
                        review.upVotes++;
                    }
                    else if (vote < 0)
                    {
                        // new vote is a downvote
                        review.downVotes++;
                    }

                    reviewUpdateTask = _businessReviewsRepository.UpdateAsync(review);

                    reviewVote.vote = vote;
                    reviewVoteUpdateTask = reviewVoteRepository.UpdateAsync(reviewVote);
                }

                if (vote == 0)
                {
                    reviewVoteUpdateTask = reviewVoteRepository.DeleteAsync(reviewVote.id, reviewVote.reviewId);
                }
            }

            Task.WaitAll(reviewUpdateTask, reviewVoteUpdateTask);
        }

        public async Task UpdateReviewAsync(Review review)
        {
            // await _reviewsRepository.UpdateAsync(review);
            throw new NotImplementedException();
        }

        public async Task UpdateReviewRequestAsync(ReviewRequest reviewRequest)
        {
            await _reviewRequestRepository.UpdateAsync(reviewRequest);
        }
        private (ReviewItem reviewItem, string reviewId, OperationType operationType) ConvertEventGridEventToReviewItem(EventGridEvent eventToProcess)
        {
            var item = new ReviewItem
            {
                Id = eventToProcess.Subject.Split('/')[1] // we assume the subject has previously been checked for its format
            };

            string reviewId;
            OperationType operationType;
            switch (eventToProcess.EventType)
            {
                //case EventTypes.Audio.AudioCreated:
                //    var audioCreatedEventData = (AudioCreatedEventData) eventToProcess.Data;
                //    item.Type = ItemType.Audio;
                //    item.Preview = audioCreatedEventData.TranscriptPreview;
                //    reviewId = audioCreatedEventData.Review;
                //    operationType = OperationType.Add;
                //    break;
                    
                //case EventTypes.Images.ImageCreated:
                //    var imageCreatedEventData = (ImageCreatedEventData) eventToProcess.Data;
                //    item.Type = ItemType.Image;
                //    item.Preview = imageCreatedEventData.PreviewUri;
                //    reviewId = imageCreatedEventData.Review;
                //    operationType = OperationType.Add;
                //    break;

                //case EventTypes.Text.TextCreated:
                //    var textCreatedEventData = (TextCreatedEventData) eventToProcess.Data;
                //    item.Type = ItemType.Text;
                //    item.Preview = textCreatedEventData.Preview;
                //    reviewId = textCreatedEventData.Review;
                //    operationType = OperationType.Add;
                //    break;

                //case EventTypes.Audio.AudioTranscriptUpdated:
                //    var audioTranscriptUpdatedEventData = (AudioTranscriptUpdatedEventData) eventToProcess.Data;
                //    item.Type = ItemType.Audio;
                //    item.Preview = audioTranscriptUpdatedEventData.TranscriptPreview;
                //    reviewId = null;
                //    operationType = OperationType.Update;
                //    break;

                //case EventTypes.Text.TextUpdated:
                //    var textUpdatedEventData = (TextUpdatedEventData) eventToProcess.Data;
                //    item.Type = ItemType.Text;
                //    item.Preview = textUpdatedEventData.Preview;
                //    reviewId = null;
                //    operationType = OperationType.Update;
                //    break;

                //case EventTypes.Audio.AudioDeleted:
                //    item.Type = ItemType.Audio;
                //    reviewId = null;
                //    operationType = OperationType.Delete;
                //    break;

                //case EventTypes.Images.ImageDeleted:
                //    item.Type = ItemType.Image;
                //    reviewId = null;
                //    operationType = OperationType.Delete;
                //    break;

                //case EventTypes.Text.TextDeleted:
                //    item.Type = ItemType.Text;
                //    reviewId = null;
                //    operationType = OperationType.Delete;
                //    break;

                default:
                    throw new ArgumentException($"Unexpected event type '{eventToProcess.EventType}' in {nameof(ProcessAddItemEventAsync)}");
            }

            if (operationType == OperationType.Add && string.IsNullOrEmpty(reviewId))
            {
                throw new InvalidOperationException("Review ID must be set for new items.");
            }
            
            return (item, reviewId, operationType);
        }

        private bool ValidateNewReview(Review review, out List<ValidationResult> validationErrors)
        {
            // validate request
            ValidationContext validationContext = new ValidationContext(review);
            validationErrors = new List<ValidationResult>();
            Validator.TryValidateObject(review, validationContext, validationErrors, true);

            //if (review == null || validationResults.Count > 0)
            //{
            //    throw new ValidationListException(validationResults);
            //}
            return validationErrors.Count == 0;
        }
    }

    public class ValidationListException : Exception
    {
        public ValidationListException(IEnumerable<ValidationResult> results)
        {
            this.ValidationResults = results;
        }

        public IEnumerable<ValidationResult> ValidationResults { get; set; }
    }
}
