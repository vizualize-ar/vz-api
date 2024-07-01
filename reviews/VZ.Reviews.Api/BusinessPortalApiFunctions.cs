using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using VZ.Reviews.Services;
using VZ.Reviews.Services.Models.Data;
using VZ.Reviews.Services.Models.Response;
using VZ.Shared;
using VZ.Shared.UserAuthentication;

namespace VZ.Reviews.Api
{


    public static class BusinessPortalApiFunctions
    {
        private static readonly IHttpAuthenticationService _authenticationService =
            new HttpAuthenticationService(Config.Auth0.BusinessPortal.Domain, Config.Auth0.BusinessPortal.ClientId, Config.Auth0.BusinessPortal.Certificate);

        [FunctionName("BP_AddReviewReply")]
        public static async Task<IActionResult> BP_AddReviewReply([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "bp/reviews/{id}/replies")]HttpRequest req, ILogger logger, string id)
        {
            if (!_authenticationService.Authenticate(req, out string auth0Id))
            {
                return new UnauthorizedResult();
            }

            try
            {
                var reviewService = new BusinessReviewsService(logger);
                var review = await reviewService.GetBusinessReviewAsync(id);
                if (!await _authenticationService.Authorize(auth0Id, review.businessId))
                {
                    return new UnauthorizedResult();
                }

                var reviewReply = JsonConvert.DeserializeObject<ReviewReply>(await req.ReadAsStringAsync());
                reviewReply.isBusiness = true;
                reviewReply.repliedById = (await ApiHelper.Instance.GetUserAsync(auth0Id)).id;

                var replyService = new ReviewReplyService(logger);
                await replyService.AddReviewReply(reviewReply);

                review.hasReplies = true;
                await reviewService.UpdateBusinessReviewAsync(review);

                return new Shared.JsonResult(reviewReply);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception");
                return new InternalServerErrorResult();
            }
        }

        [FunctionName("BP_UpdateReview")]
        public static async Task<IActionResult> BP_UpdateReview([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "bp/reviews/{id}")]HttpRequest req, string id, ILogger logger)
        {
            string businessId = req.Query["bid"];
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return new BadRequestResult();
            }
            if (!_authenticationService.AuthenticateForBusiness(req, businessId, out var userId))
            {
                return new UnauthorizedResult();
            }

            try
            {
                var reviewSummary = JsonConvert.DeserializeObject<BusinessPortalReviewSummary>(await req.ReadAsStringAsync());
                var reviewService = new BusinessReviewsService(logger);
                var review = await reviewService.GetBusinessReviewAsync(reviewSummary.id);
                review.tags = reviewSummary.tags.ToList();
                await reviewService.UpdateBusinessReviewAsync(review);

                return new NoContentResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception");
                return new InternalServerErrorResult();
            }
        }

        [FunctionName("BP_GetDashboardReviewSummary")]
        public static async Task<IActionResult> BP_GetDashboardReviewSummary([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bp/reviews/review-summary")]HttpRequest req, ILogger log)
        {
            if (!_authenticationService.Authenticate(req, out var userId))
            {
                return new UnauthorizedResult();
            }
            string businessId = req.Query["bid"];
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return new BadRequestResult();
            }
            if (!await _authenticationService.Authorize(userId, businessId))
            {
                log.LogWarning("User does not belong to this business: userId={0}, businessId={1}", userId, businessId);
                return new UnauthorizedResult();
            }

            try
            {
                var reviewsService = new BusinessReviewsService(log);
                var summary = await reviewsService.GetBusinessReviewCountByDate(businessId, DateTime.UtcNow.AddDays(-6).Date, DateTime.UtcNow.Date);
                return new Shared.JsonResult(summary);
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Unhandled exception");
                return new InternalServerErrorResult();
            }
        }

        [FunctionName("BP_GetDashboardBusinessSummary")]
        public static async Task<IActionResult> BP_GetDashboardBusinessSummary([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bp/reviews/business-summary")]HttpRequest req, ILogger log)
        {
            if (!_authenticationService.Authenticate(req, out var userId))
            {
                return new UnauthorizedResult();
            }
            string businessId = req.Query["bid"];
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return new BadRequestResult();
            }
            if (!await _authenticationService.Authorize(userId, businessId))
            {
                log.LogWarning("User does not belong to this business: userId={0}, businessId={1}", userId, businessId);
                return new UnauthorizedResult();
            }

            try
            {
                var reviewsService = new BusinessReviewsService(log);
                var summary = await reviewsService.GetBusinessReviewSummary(businessId);
                return new Shared.JsonResult(new
                {
                    summary.businessId,
                    summary.averageRating,
                    summary.totalReviews
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unhandled exception");
                return new InternalServerErrorResult();
            }
        }

        [FunctionName("BP_GetReviews")]
        public static async Task<IActionResult> BP_GetReviews([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bp/reviews")]HttpRequest req, ILogger log)
        {
            string businessId = req.Query["bid"];
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return new BadRequestResult();
            }
            if (!_authenticationService.AuthenticateForBusiness(req, businessId, out var userId))
            {
                return new UnauthorizedResult();
            }

            //if (!await _authenticationService.Authorize(userId, businessId))
            //{
            //    log.LogWarning("User does not belong to this business: userId={0}, businessId={1}", userId, businessId);
            //    return new UnauthorizedResult();
            //}

            int.TryParse(req.Query["ps"], out int pageSize);
            string sortBy = req.Query["sb"];
            string sortDirection = req.Query["sd"];
            string searchText = req.Query["q"];
            string cacheToken = req.Query["ct"];

            int? rating = null;
            if (int.TryParse(req.Query["r"], out int val))
            {
                rating = val;
            }

            bool experienceReviews = true, productReviews = true;
            if (bool.TryParse(req.Query["er"], out bool er))
            {
                experienceReviews = er;
            }
            if (bool.TryParse(req.Query["pr"], out bool pr))
            {
                productReviews = pr;
            }

            try
            {
                string continuationToken = req.GetContinuationToken();

                var key = $"{businessId}{pageSize}{sortBy}{sortDirection}{searchText}{rating ?? 0}{experienceReviews}{productReviews}{continuationToken}{cacheToken}";
                var result = CacheManager.Get<Tuple<Services.Models.Response.BusinessPortalReviewSummary[], string, int>>(key);
                if (result == null)
                {
                    var reviewsService = new BusinessReviewsService(log);
                    var queryResult = reviewsService.GetBusinessReviewsAsync(businessId, pageSize, sortBy, sortDirection, searchText, rating, experienceReviews, productReviews, continuationToken);
                    result = queryResult.ToTuple();
                    CacheManager.Set(key, result);
                }

                //var reviewsService = new ReviewsService(log);
                //var (reviews, token, total) = reviewsService.GetBusinessReviewsAsync(businessId, pageSize, sortBy, sortDirection, searchText, rating, experienceReviews, productReviews, continuationToken);
                var (reviews, token, total) = result;
                if (reviews == null)
                {
                    log.LogWarning("No reviews found for business {0}", businessId);
                    return new NotFoundResult();
                }
                if (!string.IsNullOrWhiteSpace(token))
                {
                    req.SetContinuationToken(token);
                }

                foreach (var review in reviews)
                {
                    // if (review.images == null) continue;
                    foreach (var image in review.images)
                    {
                        image.thumbpath = Config.CDN.ReviewImages + image.thumbpath;
                        image.fullpath = Config.CDN.ReviewImages + image.fullpath;
                    }
                }

                return new Shared.JsonResult(new
                {
                    reviews,
                    total
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unhandled exception");
                return new InternalServerErrorResult();
            }
        }

        [FunctionName("BP_GetReviewReplies")]
        public static async Task<IActionResult> BP_GetReviewReplies([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bp/reviews/{id}/replies")]HttpRequest req, ILogger logger, string id)
        {
            if (!_authenticationService.Authenticate(req, out string auth0Id))
            {
                return new UnauthorizedResult();
            }

            if (String.IsNullOrWhiteSpace(id))
            {
                return new BadRequestResult();
            }

            var reviewService = new BusinessReviewsService(logger);
            var review = await reviewService.GetBusinessReviewAsync(id);
            if (!await _authenticationService.Authorize(auth0Id, review.businessId))
            {
                return new UnauthorizedResult();
            }

            try
            {
                var replyService = new ReviewReplyService(logger);
                var results = await replyService.GetReviewReplies(id);
                return new Shared.JsonResult(results);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception");
                return new InternalServerErrorResult();
            }
        }

        [FunctionName("BP_GetTags")]
        public static async Task<IActionResult> BP_GetTags([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bp/reviews/tags")]HttpRequest req, ILogger log)
        {
            string businessId = req.Query["bid"];
            if (string.IsNullOrWhiteSpace(businessId))
            {
                return new BadRequestResult();
            }
            if (!_authenticationService.AuthenticateForBusiness(req, businessId, out var userId))
            {
                return new UnauthorizedResult();
            }

            try
            {
                var key = $"{businessId}_reviewTags";
                var result = CacheManager.Get<string[]>(key);
                if (result == null)
                {
                    var reviewsService = new BusinessReviewsService(log);
                    result = await reviewsService.GetBusinessReviewTagsAsync(businessId);
                    //result = new string[]
                    //{
                    //    "sql", "tasty"
                    //};
                    CacheManager.Set(key, result);
                }

                return new Shared.JsonResult(result);
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Unhandled Exception");
                return new InternalServerErrorResult();
            }
        }
    }
}
