//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.Extensions.Logging;
////using Microsoft.Azure.WebJobs.Host;
//using Newtonsoft.Json;
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web.Http;
//using VZ.Reviews.Services;
////using VZ.Reviews.Services.Converters;
//using VZ.Shared;
//using VZ.Shared.ApiAuthentication;
//using VZ.Shared.UserAuthentication;

//namespace VZ.Reviews.Api
//{
//    public static partial class ApiFunctions
//    {
//        private static readonly IHttpAuthenticationService _authenticationService = new HttpAuthenticationService(Config.Auth0.Domain, Config.Auth0.ClientId, Config.Auth0.Certificate);
//        private static readonly IInternalApiAuthenticationService _internalApiAuthenticationService = new InternalApiAuthenticationService();

//        [FunctionName("AddReview")]
//        public static async Task<IActionResult> AddReview([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "reviews")]HttpRequest req, ILogger log)
//        {
//            //// get the request body
//            //var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
//            //NewReviewRequest newReview = null;
//            //try
//            //{
//            //    newReview = JsonConvert.DeserializeObject<NewReviewRequest>(requestBody);
//            //}
//            //catch (JsonReaderException)
//            //{
//            //    return new BadRequestObjectResult(new { error = "Body should be provided in JSON format." });
//            //}

//            //var _reviewsService = new ReviewsService(log);

//            //string reviewRequestId = req.Query["rid"];
//            //if (String.IsNullOrWhiteSpace(reviewRequestId))
//            //{
//            //    try
//            //    {
//            //        if (!_authenicationService.Authenticate(req, out var userId))
//            //        {
//            //            return new UnauthorizedResult();
//            //        }
//            //    }
//            //    catch (Exception ex)
//            //    {
//            //        log.LogError("Unable to authenticate from Auth0", ex);
//            //        return new UnauthorizedResult();
//            //    }
//            //    log.LogInformation("User authenticated");

//            //    // create review
//            //    try
//            //    {
//            //        var reviewId = await _reviewsService.AddReviewAsync(newReview);
//            //        return new Shared.JsonResult(new { id = reviewId });
//            //    }
//            //    catch(ValidationListException vex)
//            //    {
//            //        JArray errors = new JArray();
//            //        foreach (var result in vex.ValidationResults)
//            //        {
//            //            errors.Add(new JValue(result.ErrorMessage));
//            //        }
//            //        return new BadRequestObjectResult(new { errors });
//            //    }
//            //    catch (Exception ex)
//            //    {
//            //        log.LogError(ex, "Unhandled exception");
//            //        return new ExceptionResult(ex, false);
//            //    }
//            //}
//            //else
//            //{
//            //    var reviewRequest = await _reviewsService.GetReviewRequestAsync(reviewRequestId);
//            //    if (reviewRequest == null)
//            //    {
//            //        return new BadRequestErrorMessageResult("Invalid review request");
//            //    }
//            //    if (!String.IsNullOrEmpty(reviewRequest.reviewId))
//            //    {
//            //        return new BadRequestErrorMessageResult("Review has already been left for this request");
//            //    }

//            //    // create review
//            //    try
//            //    {
//            //        await _reviewsService.AddReviewForRequestAsync(newReview, reviewRequest);
//            //        return new OkResult();
//            //    }
//            //    catch (ValidationListException vex)
//            //    {
//            //        JArray errors = new JArray();
//            //        foreach (var result in vex.ValidationResults)
//            //        {
//            //            errors.Add(new JValue(result.ErrorMessage));
//            //        }
//            //        return new BadRequestObjectResult(new { errors });
//            //    }
//            //    catch (Exception ex)
//            //    {
//            //        log.LogError(ex, "Unhandled exception");
//            //        return new ExceptionResult(ex, false);
//            //    }
//            //}

//            //// TODO: Implement jsonwebtoken
//            ////// get the user ID
//            ////if (! await UserAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult))
//            ////{
//            ////    return responseResult;
//            ////}

//            throw new NotImplementedException();
//        }

//        [FunctionName("DeleteReview")]
//        public static async Task<IActionResult> DeleteReview([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "reviews/{id}")]HttpRequest req, ILogger log, string id)
//        {
//            if (!_authenticationService.Authenticate(req, out var userId))
//            {
//                return new UnauthorizedResult();
//            }
            
//            // validate request
//            if (string.IsNullOrEmpty(id))
//            {
//                return new BadRequestObjectResult(new { error = "Missing required argument 'id'." });
//            }

//            // delete review
//            try
//            {
//                var _reviewsService = new BusinessReviewsService(log);
//                await _reviewsService.DeleteReviewAsync(id, userId); // we ignore the result of this call - whether it's Success or NotFound, we return an 'Ok' back to the client
//                return new NoContentResult();
//            }
//            catch (Exception ex)
//            {
//                log.LogError(ex, "Unhandled exception");
//                return new ExceptionResult(ex, false);
//            }
//        }

//        //[FunctionName("UpdateReview")]
//        //public static async Task<IActionResult> UpdateReview([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "reviews/{id}")]HttpRequest req, ILogger log, string id)
//        //{
//        //    try
//        //    {
//        //        var response = await m_httpAuthHelper.CheckIfRequestIsAuthorized(req);
//        //        if (response.Item1 == false)
//        //        {
//        //            log.LogError($"Unable to authenticate from Auth0: {response.Item2}");
//        //            return new UnauthorizedResult();

//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        log.LogError("Unable to authenticate from Auth0", ex);
//        //        return new UnauthorizedResult();
//        //    }
//        //    // get the request body
//        //    var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
//        //    UpdateReviewRequest data;
//        //    try
//        //    {
//        //        data = JsonConvert.DeserializeObject<UpdateReviewRequest>(requestBody);
//        //    }
//        //    catch (JsonReaderException)
//        //    {
//        //        return new BadRequestObjectResult(new { error = "Body should be provided in JSON format." });
//        //    }

//        //    // validate request
//        //    if (data == null)
//        //    {
//        //        return new BadRequestObjectResult(new { error = "Missing required property 'name'." });
//        //    }
//        //    if (data.Id != null && id != null && data.Id != id)
//        //    {
//        //        return new BadRequestObjectResult(new { error = "Property 'id' does not match the identifier specified in the URL path." });
//        //    }
//        //    if (string.IsNullOrEmpty(data.Id))
//        //    {
//        //        data.Id = id;
//        //    }
//        //    if (string.IsNullOrEmpty(data.Name))
//        //    {
//        //        return new BadRequestObjectResult(new { error = "Missing required property 'name'." });
//        //    }

//        //    // get the user ID
//        //    if (! await m_userAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult))
//        //    {
//        //        return responseResult;
//        //    }

//        //    // update review name
//        //    try
//        //    {
//        //        var result = await m_reviewsService.UpdateReviewAsync(data.Id, userId, data.Name);
//        //        if (result == UpdateReviewResult.NotFound)
//        //        {
//        //            return new NotFoundResult();
//        //        }

//        //        return new NoContentResult();
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        log.LogError(ex, "Unhandled exception");
//        //        return new ExceptionResult(ex, false);
//        //    }
//        //}

//        [FunctionName("GetReview")]
//        public static async Task<IActionResult> GetReview([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reviews/{id}")]HttpRequest req, ILogger log, string id)
//        {
//            // TODO: Implement authentication using a jsonwebtoken
//            //// get the user ID
//            //if (! await UserAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult))
//            //{
//            //    return responseResult;
//            //}

//            // get the review details
//            try
//            {
//                var _reviewsService = new BusinessReviewsService(log);
//                var document = await _reviewsService.GetReviewAsync(id);
//                if (document == null)
//                {
//                    return new NotFoundResult();
//                }

//                return new Shared.JsonResult(document);
//            }
//            catch (Exception ex)
//            {
//                log.LogError(ex, "Unhandled exception");
//                return new ExceptionResult(ex, false);
//            }
//        }

//        [FunctionName("ListReviews")]
//        public static async Task<IActionResult> ListReviews([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reviews")]HttpRequest req, ILogger log)
//        {
//            //// get the user ID
//            //if (! await UserAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult))
//            //{
//            //    return responseResult;
//            //}
//            var userId = "1";

//            // list the reviews
//            try
//            {
//                var _reviewsService = new BusinessReviewsService(log);
//                var reviews = await _reviewsService.GetAllReviewsAsync();
//                if (reviews == null)
//                {
//                    return new NotFoundResult();
//                }

//                // serialise the summaries using a custom converter
//                var settings = new JsonSerializerSettings
//                {
//                    NullValueHandling = NullValueHandling.Ignore,
//                    Formatting = Formatting.Indented
//                };
//                //settings.Converters.Add(new CategorySummariesConverter());
//                //var json = JsonConvert.SerializeObject(reviews, settings);
//                var json = JsonConvert.SerializeObject(reviews);

//                return new Shared.JsonResult(json);
//            }
//            catch (Exception ex)
//            {
//                log.LogError(ex, "Unhandled exception");
//                return new ExceptionResult(ex, false);
//            }
//        }

//        [FunctionName("Health")]
//        public static IActionResult Health([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")]HttpRequest req, ILogger log)
//        {
//            return new OkResult();
//        }

//        //[FunctionName("KeepAliveTimer")]
//        //public static void KeepAlive([TimerTrigger("0 */4 * * * *")] TimerInfo myTimer, ILogger log)
//        //{
//        //    log.LogTrace("keep warm");
//        //}

//        [FunctionName("GetReviewRequests")]
//        public static async Task<IActionResult> GetReviewRequests([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "review-requests/byorder")]HttpRequest req, ILogger log)
//        {
//            if (!_internalApiAuthenticationService.Authenticate(req, log))
//            {
//                return new UnauthorizedResult();
//            }

//            try
//            {
//                long[] orderIds = req.Query["oids"].ToString().Split(',').Cast<long>().ToArray();

//                var reviewsService = new BusinessReviewsService(log);
//                var reviewRequests = await reviewsService.GetReviewRequestsForOrdersAsync(orderIds);

//                return new Shared.JsonResult(reviewRequests);
//            }
//            catch(Exception ex)
//            {
//                log.LogError(ex, "Unhandlec exception");
//                return new InternalServerErrorResult();
//            }
//        }
//    }   
//}
