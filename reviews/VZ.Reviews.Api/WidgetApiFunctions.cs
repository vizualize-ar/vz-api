//using Flurl.Http;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web.Http;
//using VZ.Reviews.Services;
//using VZ.Reviews.Services.Models.Data;
//using VZ.Reviews.Services.Models.Results;
//using VZ.Shared;
//using VZ.Shared.ApiAuthentication;
//using VZ.Shared.UserAuthentication;

//namespace VZ.Reviews.Api
//{
//    public static partial class ApiFunctions
//    {
//        private static IApiAuthenticationService _apiAuthenticationService = new ApiAuthenticationService(Config.Api.BaseUrl);
//        //private static readonly IReviewsService _reviewsService = new ReviewsService(new EventGridPublisherService(), new ReviewsRepository(), new BusinessReviewSummaryRepository(), new BusinessReviewsRepository());

//        [FunctionName("Widgets_ListBusinessReviews")]
//        public static async Task<IActionResult> Widgets_ListBusinessReviews([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "w/reviews")]HttpRequest req, ILogger log)
//        {
//            try
//            {
//                if (!_apiAuthenticationService.Authenticate(req, out string businessId, log))
//                {
//                    return new UnauthorizedResult();
//                }

//                if (string.IsNullOrWhiteSpace(req.Query["rt"]))
//                {
//                    log.LogWarning("Review type (rt) parameter is missing");
//                    return new BadRequestResult();
//                }
//                if (string.IsNullOrWhiteSpace(req.Query["wid"]))
//                {
//                    log.LogWarning("Widget ID (wid) parameter is missing");
//                    return new BadRequestResult();
//                }

//                // get review type parameter
//                if (!Enum.TryParse(typeof(ReviewType), req.Query["rt"], out object rt))
//                {
//                    return new BadRequestResult();
//                }
//                ReviewType reviewType = (ReviewType)rt;

//                // get widget id parameter
//                string widgetId = req.Query["wid"];

//                // get business product ID parameter, if applicable
//                string externalProductId = req.Query["pid"];
//                int.TryParse(req.Query["r"], out int rating);

//                string[] tags = null;
//                if (string.IsNullOrWhiteSpace(req.Query["t"]) == false)
//                {
//                    tags = req.Query["t"].ToString().Split(',');
//                }

//                #region Save client info

//                var requestSummary = req.GetSummary();
//                var publisher = new EventGridPublisherService(log);
//                publisher.PostEventGridEventAsync(EventTypes.Widgets.WidgetViewed, subject: widgetId, payload: requestSummary);

//                #endregion


//                Shared.Models.BusinessWidget widget = null;
//                try
//                {
//                    var key = $"{req.Headers["Referer"]}_{req.Headers["Authorization"]}_{widgetId}";
//                    widget = Shared.CacheManager.Get(key) as Shared.Models.BusinessWidget;
//                    if (widget == null)
//                    {
//                        widget = await $"{Environment.GetEnvironmentVariable("ApiBaseUrl")}/w/widgets/{widgetId}"
//                        .WithHeader("Authorization", req.Headers["Authorization"])
//                        .WithHeader("Referer", req.Headers["Referer"])
//                        .GetJsonAsync<Shared.Models.BusinessWidget>();

//                        Shared.CacheManager.Set(key, widget);
//                    }
//                }
//                catch (FlurlHttpException fex) when (fex.Call.HttpStatus == System.Net.HttpStatusCode.NotFound)
//                {
//                    log.LogWarning("Widget not found for business. Business ID: {0}, Widget ID: {1}", businessId, widgetId);
//                    return new BadRequestResult();
//                }

//                var reviewsService = new BusinessReviewsService(log);

//                // get business summary
//                var summaryTask = reviewsService.GetBusinessReviewSummary(businessId, externalProductId);

//                bool allowPaging = false;
//                int pageSize = 12;
//                if (widget.widgetType == Shared.Models.WidgetType.Feed)
//                {
//                    pageSize = 20;
//                    allowPaging = true;
//                }
//                string continuationToken = req.GetContinuationToken();

//                var reviewsTask = reviewsService.GetWidgetBusinessReviewsAsync(reviewType, businessId, externalProductId, pageSize, rating, tags, continuationToken);
//                await Task.WhenAll(summaryTask, reviewsTask);

//                // Only send back the continuation token if paging is allowed for that widget
//                if (allowPaging && !String.IsNullOrWhiteSpace(reviewsTask.Result.Item2))
//                {
//                    //req.HttpContext.Response.Headers["x-tr-continuation"] = reviewsTask.Result.Item2;
//                    req.SetContinuationToken(reviewsTask.Result.Item2);
//                }

//                var reviews = reviewsTask.Result.Item1;
//                for (int i = 0; i < reviews.Length; i++)
//                {                    
//                    if (reviews[i].thumbnails.Length > 0)
//                    {
//                        for (int img = 0; img < reviews[i].thumbnails.Length; img++)
//                        {
//                            reviews[i].thumbnails[img] = Config.CDN.ReviewImages + reviews[i].thumbnails[img];
//                        }
//                    }
//                }

//                var response = new Services.Models.Results.WidgetReviewsResult()
//                {
//                    summary = new WidgetBusinessSummary
//                    {
//                        averageRating = summaryTask.Result.averageRating,
//                        slug = summaryTask.Result.slug,
//                        totalReviews = summaryTask.Result.totalReviews
//                    },
//                    reviews = reviews
//                };
//                return new Shared.JsonResult(response);
//            }
//            catch (Exception ex)
//            {
//                log.LogError(ex, "Unhandled exception");
//                return new InternalServerErrorResult();
//            }
//        }

//        /// <summary>
//        /// Vote on a review. Requires user to be logged in and have a valid Authorization header containing an Auth0 JWT
//        /// </summary>
//        /// <param name="req"></param>
//        /// <param name="log"></param>
//        /// <param name="id"></param>
//        /// <returns></returns>
//        [FunctionName("Widgets_ReviewVote")]
//        public static async Task<IActionResult> Widgets_ReviewVote([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "w/reviews/{id}/vote")]HttpRequest req, ILogger log, string id)
//        {
//            try
//            {
//                var userAuthenticationService = new HttpAuthenticationService(Config.Auth0.Domain, Config.Auth0.ConsumerPortalClientID, Config.Auth0.Certificate);
//                if (!userAuthenticationService.Authenticate(req, out string userId))
//                {
//                    return new UnauthorizedResult();
//                }

//                if (String.IsNullOrWhiteSpace(id))
//                {
//                    log.LogInformation("Review ID parameter is missing. Path segment reviews/{id}/vote");
//                    return new BadRequestResult();
//                }
//                if (!short.TryParse(req.Query["v"], out short vote))
//                {
//                    log.LogInformation("Vote paramter is missing. Query parameter field 'v'.");
//                    return new BadRequestResult();
//                }

//                var reviewsService = new BusinessReviewsService(log);
//                await reviewsService.UpdateBusinessReviewVote(id, userId, vote);
//                return new OkResult();
//            }
//            catch (Exception ex)
//            {
//                log.LogError(ex, "Unhandled exception");
//                return new InternalServerErrorResult();
//            }
//        }

//        [FunctionName("Widgets_GetBusinessReviewSummary")]
//        public static async Task<IActionResult> Widgets_GetBusinessReviewSummary([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "w/reviews/summary")]HttpRequest req, ILogger log)
//        {
//            try
//            {
//                if (!_apiAuthenticationService.Authenticate(req, out string businessId, log))
//                {
//                    return new UnauthorizedResult();
//                }
//                if (string.IsNullOrWhiteSpace(req.Query["wid"]))
//                {
//                    log.LogWarning("Widget ID (wid) parameter is missing");
//                    return new BadRequestResult();
//                }

//                // get widget id parameter
//                string widgetId = req.Query["wid"];

//                #region Save client info

//                var requestSummary = req.GetSummary();
//                var publisher = new EventGridPublisherService(log);
//                publisher.PostEventGridEventAsync(EventTypes.Widgets.WidgetViewed, subject: widgetId, payload: requestSummary);

//                #endregion

//                Shared.Models.BusinessWidget widget = null;
//                try
//                {
//                    var key = $"{req.Headers["Referer"]}_{req.Headers["Authorization"]}_{widgetId}";
//                    widget = Shared.CacheManager.Get(key) as Shared.Models.BusinessWidget;
//                    if (widget == null)
//                    {
//                        widget = await $"{Environment.GetEnvironmentVariable("ApiBaseUrl")}/w/widgets/{widgetId}"
//                        .WithHeader("Authorization", req.Headers["Authorization"])
//                        .WithHeader("Referer", req.Headers["Referer"])
//                        .GetJsonAsync<Shared.Models.BusinessWidget>();

//                        Shared.CacheManager.Set(key, widget);
//                    }
//                }
//                catch (FlurlHttpException fex) when (fex.Call.HttpStatus == System.Net.HttpStatusCode.NotFound)
//                {
//                    log.LogWarning("Widget not found for business. Business ID: {0}, Widget ID: {1}", businessId, widgetId);
//                    return new BadRequestResult();
//                }

//                var reviewsService = new BusinessReviewsService(log);

//                // get business summary
//                var summary = await reviewsService.GetBusinessReviewSummary(businessId);
//                WidgetBusinessSummary response = new WidgetBusinessSummary
//                {
//                    averageRating = summary.averageRating,
//                    slug = summary.slug,
//                    totalReviews = summary.totalReviews
//                };
//                return new Shared.JsonResult(response);
//            }
//            catch (Exception ex)
//            {
//                log.LogError(ex, "Unhandled exception");
//                return new InternalServerErrorResult();
//            }
//        }

//        //[FunctionName("Widgets_ListBusinessReviews")]
//        ////public static async Task<IActionResult> Widgets_ListBusinessReviews([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "w/reviews")]HttpRequest req, ILogger log)
//        //public static async Task<HttpResponseMessage> Widgets_ListBusinessReviews([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "w/reviews")]HttpRequestMessage req, ILogger log)
//        //{
//        //    Stopwatch allWatch = new Stopwatch();
//        //    allWatch.Start();
//        //    try
//        //    {
//        //        Stopwatch stopWatch = new Stopwatch();
//        //        stopWatch.Start();

//        //        //HttpRequest req2 = HttpRequest
//        //        //if (!_apiAuthenticationService.Authenticate(req, out string businessId))
//        //        //{
//        //        //    return new UnauthorizedResult();
//        //        //}
//        //        //log.LogInformation("authentication: {0}", stopWatch.ElapsedMilliseconds);

//        //        stopWatch.Restart();
//        //        //if (string.IsNullOrWhiteSpace(req.Query["rt"]))
//        //        var query = req.RequestUri.ParseQueryString();
//        //        if (string.IsNullOrWhiteSpace(query["rt"]))
//        //        {
//        //            log.LogWarning("Review type (rt) parameter is missing");
//        //            //return new BadRequestResult();
//        //            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
//        //        }
//        //        //if (string.IsNullOrWhiteSpace(req.Query["wid"]))
//        //        if (string.IsNullOrWhiteSpace(query["wid"]))
//        //        {
//        //            log.LogWarning("Widget ID (wid) parameter is missing");
//        //            //return new BadRequestResult();
//        //            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
//        //        }

//        //        // get review type parameter
//        //        //if (!Enum.TryParse(typeof(ReviewType), req.Query["rt"], out object rt))
//        //        if (!Enum.TryParse(typeof(ReviewType), query["rt"], out object rt))
//        //        {
//        //            //return
//        //            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest); new BadRequestResult();
//        //        }
//        //        ReviewType reviewType = (ReviewType)rt;

//        //        // get widget id parameter
//        //        //string widgetId = req.Query["wid"];
//        //        string widgetId = query["wid"];

//        //        // get business product ID parameter, if applicable
//        //        //string businessProductId = req.Query["bpid"];
//        //        string businessProductId = query["bpid"];

//        //        log.LogInformation("Checking parameters: {0}", stopWatch.ElapsedMilliseconds);

//        //        Shared.Models.BusinessWidget widget = null;
//        //        try
//        //        {
//        //            stopWatch.Restart();
//        //            //var key = $"{req.Headers["Referer"]}_{req.Headers["Authorization"]}_{widgetId}";
//        //            var key = $"{req.Headers.Referrer}_{req.Headers.Authorization}_{widgetId}";
//        //            widget = Shared.CacheManager.Get(key) as Shared.Models.BusinessWidget;
//        //            if (widget == null)
//        //            {
//        //                widget = await $"{Environment.GetEnvironmentVariable("ApiBaseUrl")}/w/widgets/{widgetId}"
//        //                //.WithHeader("Authorization", req.Headers["Authorization"])
//        //                //.WithHeader("Referer", req.Headers["Referer"])
//        //                .WithHeader("Authorization", req.Headers.Authorization)
//        //                .WithHeader("Referer", req.Headers.Referrer)
//        //                .GetJsonAsync<Shared.Models.BusinessWidget>();

//        //                Shared.CacheManager.Set(key, widget);
//        //            }
//        //            log.LogInformation("widget verification: {0}", stopWatch.ElapsedMilliseconds);
//        //        }
//        //        catch (FlurlHttpException fex) when (fex.Call.HttpStatus == System.Net.HttpStatusCode.NotFound)
//        //        {
//        //            //log.LogWarning("Widget not found for business. Business ID: {0}, Widget ID: {1}", businessId, widgetId);
//        //            //return new BadRequestResult();
//        //            return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
//        //        }

//        //        //// get business summary
//        //        //var summaryTask = _reviewsService.GetBusinessReviewSummary(businessId);

//        //        //int pageSize = 12;
//        //        //if (widget.widgetType == Shared.Models.WidgetType.Feed)
//        //        //{
//        //        //    pageSize = 20;
//        //        //}
//        //        //string continuationToken = req.Headers["x-continuationtoken"];
//        //        //var reviewsTask = _reviewsService.GetBusinessReviewsAsync(reviewType, businessId, businessProductId, pageSize, continuationToken);

//        //        //await Task.WhenAll(summaryTask, reviewsTask);

//        //        //if (!String.IsNullOrWhiteSpace(reviewsTask.Result.Item2))
//        //        //{
//        //        //    req.HttpContext.Response.Headers["x-continuationtoken"] = reviewsTask.Result.Item2;
//        //        //}

//        //        //var reviews = new WidgetReviews[reviewsTask.Result.Item1.Length];
//        //        //for(int i = 0; i < reviewsTask.Result.Item1.Length; i++)
//        //        //{
//        //        //    var rev = reviewsTask.Result.Item1[i];
//        //        //    reviews[i] = new WidgetReviews
//        //        //    {
//        //        //        body = rev.body,
//        //        //        downVotes = rev.downVotes,
//        //        //        rating = rev.rating,
//        //        //        tamperproof = rev.tamperproof,
//        //        //        title = rev.title,
//        //        //        upVotes = rev.upVotes,
//        //        //        avatar = rev.user.avatarUrl,
//        //        //        user = rev.user.name,
//        //        //        verified = rev.verified,
//        //        //        createdOn = new DateTimeOffset(rev.createdOn).ToUnixTimeMilliseconds()
//        //        //    };
//        //        //}

//        //        //var response = new Services.Models.Results.WidgetReviewsResult()
//        //        //{
//        //        //    summary = new WidgetBusinessSummary
//        //        //    {
//        //        //        averageRating = summaryTask.Result.averageRating,
//        //        //        slug = summaryTask.Result.slug,
//        //        //        totalReviews = summaryTask.Result.totalReviews
//        //        //    },
//        //        //    reviews = reviews
//        //        //};
//        //        //return new Shared.JsonResult(response);

//        //        var businessId = "6ec007798715";
//        //        stopWatch.Restart();
//        //        var result = await _reviewsService.GetBusinessReviewsAsync(reviewType, businessId, businessProductId, 20, null);
//        //        log.LogInformation("reviews retrieval: {0}", stopWatch.ElapsedMilliseconds);
//        //        //var response = new Shared.JsonResult(result);
//        //        log.LogInformation("Entire function run time: {0}", allWatch.ElapsedMilliseconds);
//        //        //return response;
//        //        return req.CreateResponse<BusinessReview[]>(HttpStatusCode.OK, result.Item1);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        log.LogError(ex, "Unhandled exception");
//        //        //return new InternalServerErrorResult();
//        //        return req.CreateErrorResponse(HttpStatusCode.InternalServerError, "Server error");
//        //    }
//        //}
//    }
//}