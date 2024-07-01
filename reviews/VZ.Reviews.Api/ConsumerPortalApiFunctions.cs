using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using VZ.Shared;
using VZ.Reviews.Services;

namespace VZ.Reviews.Api
{
    using Services.Models.Request;
    using Services.Models.Request.ConsumerPortal;
    using VZ.Reviews.Services.Models.Data;

    public class ConsumerPortalApiFunctions
    {
        [FunctionName("CP_AddReviewForProductReviewRequest")]
        public static async Task<IActionResult> CP_AddReviewForProductReviewRequest([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cp/reviews/product")]HttpRequest req, ILogger log)
        {
            // get the request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            NewReviewRequest newReview = null;
            try
            {
                newReview = JsonConvert.DeserializeObject<NewReviewRequest>(requestBody);
            }
            catch (JsonReaderException)
            {
                return new BadRequestObjectResult(new { error = "Body should be provided in JSON format." });
            }

            var _reviewsService = new BusinessReviewsService(log);

            string reviewRequestId = req.Query["rid"];
            var reviewRequest = await _reviewsService.GetReviewRequestAsync(reviewRequestId);
            if (reviewRequest == null)
            {
                return new BadRequestErrorMessageResult("Invalid review request");
            }
            if (reviewRequest.IsProductReviewed(newReview.productId))
            {
                return new BadRequestErrorMessageResult("Review has already been left for this request");
            }

            // create review
            try
            {
                await _reviewsService.AddReviewForRequestAsync(newReview, reviewRequest);
                return new OkResult();
            }
            catch (ValidationListException vex)
            {
                JArray errors = new JArray();
                foreach (var result in vex.ValidationResults)
                {
                    errors.Add(new JValue(result.ErrorMessage));
                }
                return new BadRequestObjectResult(new { errors });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unhandled exception");
                return new ExceptionResult(ex, false);
            }

            // TODO: Implement jsonwebtoken
            //// get the user ID
            //if (! await UserAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult))
            //{
            //    return responseResult;
            //}
        }

        [FunctionName("CP_AddReviewForExperienceReviewRequest")]
        public static async Task<IActionResult> CP_AddReviewForExperienceReviewRequest([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cp/reviews/experience")]HttpRequest req, ILogger log)
        {
            // get the request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            NewReviewRequest newReview = null;
            try
            {
                newReview = JsonConvert.DeserializeObject<NewReviewRequest>(requestBody);
            }
            catch (JsonReaderException)
            {
                return new BadRequestObjectResult(new { error = "Body should be provided in JSON format." });
            }

            var _reviewsService = new BusinessReviewsService(log);

            string reviewRequestId = req.Query["rid"];
            var reviewRequest = await _reviewsService.GetReviewRequestAsync(reviewRequestId);
            if (reviewRequest == null)
            {
                return new BadRequestErrorMessageResult("Invalid review request");
            }

            // create review
            try
            {
                await _reviewsService.AddReviewForRequestAsync(newReview, reviewRequest);
                return new OkResult();
            }
            catch (ValidationListException vex)
            {
                JArray errors = new JArray();
                foreach (var result in vex.ValidationResults)
                {
                    errors.Add(new JValue(result.ErrorMessage));
                }
                return new BadRequestObjectResult(new { errors });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unhandled exception");
                return new ExceptionResult(ex, false);
            }
        }

        [FunctionName("CP_GetReviewRequest")]
        public static IActionResult CP_GetReviewRequest([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cp/reviews/requests/{id}")]HttpRequest req, ILogger log, string id)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return new BadRequestObjectResult(new { error = "Missing required argument 'id'." });
            }
            if (!Enum.TryParse(typeof(ReviewType), req.Query["t"], out object rt))
            {
                log.LogError("Invalid review type for request. id: {0}, t: {1}", id, req.Query["t"]);
                return new BadRequestResult();
            }
            var reviewType = (ReviewType)rt;

            try
            {
                var _reviewsService = new BusinessReviewsService(log);
                var reviewRequest = _reviewsService.GetReviewRequestResponse(id, reviewType);

                // remove any product reviews already fulfilled
                if (reviewRequest.type == Services.Models.Data.ReviewType.BusinessProduct)
                {
                    reviewRequest.products.RemoveAll(p => p.reviewId != null);
                    if (reviewRequest.products.Count == 0)
                    {
                        reviewRequest.status = "completed";
                    }
                }
                else if (reviewRequest.type == Services.Models.Data.ReviewType.Business)
                {
                    if (reviewRequest.reviewId != null)
                    {
                        reviewRequest.status = "completed";
                    }
                }

                return new Shared.JsonResult(reviewRequest);
            }
            catch (ArgumentException ex)
            {
                log.LogError(ex, "Error downstream. ID={0}", id);
                return new BadRequestErrorMessageResult("Invalid request");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error retrieving review request: {0}", id);
                return new ExceptionResult(ex, false);
            }
        }

        [FunctionName("CP_GetBusinessReviews")]
        public static async Task<IActionResult> CP_GetBusinessReviews([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cp/reviews/business/{id}")] HttpRequest req, ILogger log, string id)
        {
            try
            {
                var reviewsService = new CPBusinessReviewsService(log);
                string singleReviewId = req.Query["rid"];
                int.TryParse(req.Query["rt"], out int ratingFilter);
                int.TryParse(req.Query["ps"], out int pageSize);
                Enum.TryParse<SortField>(req.Query["sf"], out SortField sortField);
                string continuationToken = req.GetContinuationToken();
                var results = await reviewsService.GetBusinessReviewsAsync(id, singleReviewId, ratingFilter, sortField, pageSize, continuationToken);
                req.SetContinuationToken(results.continuationToken);
                return new Shared.JsonResult(results.reviews);
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Unhandlex exception");
                return new ExceptionResult(ex, false);
            }
        }

        [FunctionName("CP_CreateImageBlobPlaceholders")]
        public static async Task<IActionResult> CP_CreateImageBlobPlaceholders([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cp/reviews/requests/{id}/images/placeholders")] HttpRequest req, ILogger log, string id)
        {
            try
            {
                var _reviewsService = new BusinessReviewsService(log);
                var reviewRequest = await _reviewsService.GetReviewRequestAsync(id);
                if (null == reviewRequest)
                {
                    log.LogWarning("Review request not found: {0}", id);
                    return new BadRequestResult();
                }

                var request = JsonConvert.DeserializeObject<NewReviewRequestImage>(await req.ReadAsStringAsync());
                
                // experience reviews don't have image upload
                // for product review: upload image to //blogstorage/[reviewRequestId]/[productId]/
                var imageService = new ImageService(log);
                var tokenUrls = new List<dynamic>();
                foreach(var imageName in request.images)
                {
                    string placeHolderName = Guid.NewGuid().ToString() + Path.GetExtension(imageName);
                    var result = imageService.BeginAddImage(Config.Blob.ReviewRequestMediaContainer, id, request.productId, placeHolderName);
                    tokenUrls.Add(new
                    {
                        result.id,
                        result.url
                    });
                }

                return new Shared.JsonResult(tokenUrls);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unhandled exception");
                return new ExceptionResult(ex, false);
            }
        }
    }
}
