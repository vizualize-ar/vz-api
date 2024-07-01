//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Azure.EventGrid;
//using Microsoft.Azure.EventGrid.Models;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.EventGrid;
//using Microsoft.Azure.WebJobs.Extensions.Http;
//using Microsoft.Azure.WebJobs.Host;
//using Microsoft.Extensions.Logging;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using System.Web.Http;
//using TrueRevue.Reviews.Services;
//using TrueRevue.Reviews.Services.Repositories;
//using TrueRevue.Shared;
//using TrueRevue.Shared.EventSchemas.Reviews;

//namespace TrueRevue.Reviews.WorkerApi
//{
//    public static partial class WorkerApiFunctions
//    {
//        private static readonly IEventGridSubscriberService _eventGridSubscriberService = new EventGridSubscriberService();

//        [FunctionName("AddReviewItem")]
//        public static async Task<IActionResult> AddReviewItem([HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequest req, ILogger log)
//        {
//            // authenticate to Event Grid if this is a validation event
//            var eventGridValidationOutput = _eventGridSubscriberService.HandleSubscriptionValidationEvent(req);
//            if (eventGridValidationOutput != null)
//            {
//                log.LogInformation("Responding to Event Grid subscription verification.");
//                return eventGridValidationOutput;
//            }

//            try
//            {
//                var (eventGridEvent, userId, _) = _eventGridSubscriberService.DeconstructEventGridMessage(req);

//                var reviewsService = new ReviewsService(log);
//                // process the category item
//                await reviewsService.ProcessAddItemEventAsync(eventGridEvent);

//                return new OkResult();
//            }
//            catch (Exception ex)
//            {
//                log.LogError(ex, "Unhandled exception");
//                return new ExceptionResult(ex, false);
//            }
//        }

//        //[FunctionName("UpdateReviewItem")]
//        //public static async Task<IActionResult> UpdateReviewItem([HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequest req, ILogger log)
//        //{
//        //    // authenticate to Event Grid if this is a validation event
//        //    var eventGridValidationOutput = EventGridSubscriberService.HandleSubscriptionValidationEvent(req);
//        //    if (eventGridValidationOutput != null)
//        //    {
//        //        log.LogInformation("Responding to Event Grid subscription verification.");
//        //        return eventGridValidationOutput;
//        //    }

//        //    try
//        //    {
//        //        var (eventGridEvent, userId, _) = EventGridSubscriberService.DeconstructEventGridMessage(req);

//        //        // process the category item
//        //        await CategoriesService.ProcessUpdateItemEventAsync(eventGridEvent, userId);

//        //        return new OkResult();
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        log.LogError(ex, "Unhandled exception");
//        //        return new ExceptionResult(ex, false);
//        //    }
//        //}

//        [FunctionName("DeleteReviewItem")]
//        public static async Task<IActionResult> DeleteReviewItem([HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequest req, ILogger log)
//        {
//            // authenticate to Event Grid if this is a validation event
//            var eventGridValidationOutput = _eventGridSubscriberService.HandleSubscriptionValidationEvent(req);
//            if (eventGridValidationOutput != null)
//            {
//                log.LogInformation("Responding to Event Grid subscription verification.");
//                return eventGridValidationOutput;
//            }

//            try
//            {
//                var (eventGridEvent, userId, _) = _eventGridSubscriberService.DeconstructEventGridMessage(req);

//                var reviewsService = new ReviewsService(log);
//                // process the category item
//                await reviewsService.ProcessDeleteItemEventAsync(eventGridEvent, userId);

//                return new OkResult();
//            }
//            catch (Exception ex)
//            {
//                log.LogError(ex, "Unhandled exception");
//                return new ExceptionResult(ex, false);
//            }
//        }

//        //TODO add this method back after adding image service to ReviewServiceClass

//        //[FunctionName("AddCategoryImage")]
//        //public static async Task<IActionResult> AddCategoryImage(
//        //    [HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequest req,
//        //    TraceWriter log)
//        //{
//        //    // authenticate to Event Grid if this is a validation event
//        //    var eventGridValidationOutput = EventGridSubscriberService.HandleSubscriptionValidationEvent(req);
//        //    if (eventGridValidationOutput != null)
//        //    {
//        //        log.Info("Responding to Event Grid subscription verification.");
//        //        return eventGridValidationOutput;
//        //    }

//        //    try
//        //    {
//        //        var (_, userId, categoryId) = EventGridSubscriberService.DeconstructEventGridMessage(req);

//        //        // process the category image
//        //        log.Info($"Updating image for category ID {categoryId}...");
//        //        var updated = await CategoriesService.UpdateCategoryImageAsync(categoryId, userId);
//        //        if (!updated)
//        //        {
//        //            log.Warning("Did not update category image as no images were available.");
//        //        }

//        //        return new OkResult();
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        log.Error("Unhandled exception", ex);
//        //        return new ExceptionResult(ex, false);
//        //    }
//        //}
//    }
//}
