using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using VZ.Membership.Services;
using VZ.Membership.Services.Models.Data;
using VZ.Orders.Services;
using VZ.Orders.Services.Models.Data;
using VZ.Shared;
using VZ.Shared.Data;
using VZ.Shared.EventSchemas.Products;
using VZ.Shared.Queueing;
using VZ.Shared.Storage;

namespace VZ.WorkerApi
{
    public static class WorkerApiFunctions
    {
        private static readonly IEventGridSubscriberService _eventGridSubscriberService = new EventGridSubscriberService();

        #region API Functions

        [EventHandler(EventTypes.Products.ProductViewed)]
        [FunctionName("ProcessProductViewedEvent")]
        public static async Task<IActionResult> ProcessProductViewedEvent([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ProcessProductViewedEvent")] HttpRequest req, ILogger log)
        {
            if (_eventGridSubscriberService.HandleSubscriptionValidationEvent(req, out var eventGridValidationOutput))
            {
                log.LogInformation("Responding to Event Grid subscription verification.");
                return eventGridValidationOutput;
            }

            var eventGridEvent = req.ToEvent<ProductViewedEventData>();
            log.LogDebug("Product viewed: {0}", eventGridEvent.Data.productId);

            var businessProductService = new BusinessProductService(log);
            await businessProductService.AddProductViewAsync(eventGridEvent.Data.businessId, eventGridEvent.Data.productId, eventGridEvent.Data.requestSummary);

            return new OkResult();
        }

        #endregion API Functions

        #region Private Methods

        private static async Task<Tuple<BusinessCustomer, List<BusinessProduct>>> EnsureBusinessEntitiesExist(ILogger logger, BusinessOrder order, Business business)
        {
            //var businessCustomerService = new BusinessCustomerService();
            //var customer = await businessCustomerService.GetAsync(order.customer.email, order.businessId);
            //if (customer == null)
            //{
            //    customer = new BusinessCustomer()
            //    {
            //        businessId = order.businessId,
            //        email = order.customer.email,
            //        firstName = order.customer.firstName,
            //        lastName = order.customer.lastName,
            //        phone = order.customer.phone
            //    };
            //    var user = await CreateUserForBusinessCustomer(customer, logger);
            //    customer.userId = user.id;

            //    await businessCustomerService.AddAsync(customer);

            //}

            //var businessProductService = new BusinessProductService(logger);
            //List<BusinessProduct> products = new List<BusinessProduct>();
            //foreach (var orderItem in order.items)
            //{
            //    var product = await businessProductService.GetByExternalIdAsync(orderItem.productId, order.businessId);
            //    if (product == null)
            //    {
            //        //product = new BusinessProduct(order.businessId)
            //        product = new BusinessProduct()
            //        {
            //            businessId = order.businessId,
            //            // images = orderItem.images,
            //            images = new List<BusinessProductImage>(),
            //            name = orderItem.name,
            //            sku = orderItem.sku,
            //            upc = orderItem.upc,
            //            externalProductId = orderItem.productId
            //        };

            //        //product.id = await businessProductService.AddAsync(product);
            //        await businessProductService.AddAsync(product);

            //        // Download images
            //        foreach (var imageUrl in orderItem.images)
            //        {
            //            //string folder = $"{order.businessId}/{product.id.IdPart()}";
            //            string folder = $"{order.businessId}/{product.id}";
            //            using (var stream = await imageUrl.GetStreamAsync())
            //            {
            //                IBlobService blobService = new BlobService(logger);
            //                var cloudBlockBlob = blobService.CreatePlaceholderBlob(Config.Blob.BusinessProductMediaContainer, folder, Guid.NewGuid() + Path.GetExtension(imageUrl));
            //                cloudBlockBlob.UploadFromStream(stream);
            //                product.images.Add(new BusinessProductImage
            //                {
            //                    fullpath = cloudBlockBlob.Uri.AbsoluteUri
            //                });
            //            }
            //        }
            //        await businessProductService.UpdateAsync(product);
            //    }
            //    products.Add(product);
            //}

            //return Tuple.Create(customer, products);
            throw new NotImplementedException("Part of TR");
        }

        //private static async Task<User> CreateUserForBusinessCustomer(BusinessCustomer customer, ILogger log)
        //{
        //    var userService = new CPUserService(log);
        //    var user = await userService.GetByEmailAsync(customer.email);
        //    if (user == null)
        //    {
        //        user = new User();
        //        user.email = customer.email;
        //        user.firstName = customer.firstName;
        //        user.lastName = customer.lastName;
        //        user.phone = customer.phone;
        //        await userService.AddUserAsync(user);
        //    }

        //    return user;
        //}

        //private static async Task ProcessBusinessReviewRequest(ILogger log, IBusinessReviewsService reviewService, Business business, BusinessOrder order, BusinessCustomer customer)
        //{
        //    var reviewRequest = await reviewService.GetExperienceReviewRequestAsync(order.businessId, order.orderId);
        //    if (reviewRequest == null)
        //    {
        //        reviewRequest = new ReviewRequest(business.id)
        //        {
        //            businessId = business.id,
        //            businessLogoUrl = business.logoUrl,
        //            orderId = order.id,
        //            businessOrderId = order.orderId,
        //            customerId = customer.id,
        //            userId = customer.userId,
        //            type = ReviewType.Business,
        //            sendOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        //        };
        //        var requestId = await reviewService.AddReviewRequestAsync(reviewRequest);
        //        reviewRequest = await reviewService.GetReviewRequestAsync(requestId);
        //    }
        //    else
        //    {
        //        // Review request already been sent
        //        if (reviewRequest.sentOn > 0)
        //        {
        //            log.LogError("Experience review request was already created for this order. Business ID={0}, Order ID={1}, ReviewRequest ID={2}", order.businessId, order.id, reviewRequest.id);
        //            //return new OkResult();
        //            return;
        //        }

        //        // Review request was already fulfulled (review left)
        //        if (!String.IsNullOrWhiteSpace(reviewRequest.reviewId))
        //        {
        //            log.LogError("Experience review was left for this order. Business ID={0}, Order ID={1}, Request ID={2}, Review ID={3}", order.businessId, order.id, reviewRequest.id, reviewRequest.reviewId);
        //            //return new OkResult();
        //            return;
        //        }
        //    }


        //    Emailer emailer = new Emailer(Config.Email.PublicKey, Config.Email.PrivateKey, Config.Email.FromEmail, Config.Email.FromName, log);
        //    emailer
        //        .AddVar("businessLogoUrl", business.logoUrl)
        //        .AddVar("businessName", business.name)
        //        .AddVar("firstName", order.customer.firstName)
        //        .AddVar("reviewRequestUrl", $"{Config.ConsumerPortalUrl}/business/submit-review?id={reviewRequest.id}&t={(int)reviewRequest.type}");
        //    var recipients = new List<EmailMessageAddress>
        //        {
        //            new EmailMessageAddress(order.customer.email, order.customer.firstName)
        //        };
        //    var response = await emailer.SendEmail(Config.Email.Template.BusinessReviewRequest, "We would like your feedback", recipients);

        //    reviewRequest.sentOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        //    reviewRequest.emailMessageId = response.Messages[0].To[0].MessageID;
        //    await reviewService.UpdateReviewRequestAsync(reviewRequest);

        //    // Update order Experience ReviewRequest sentOn timestamp
        //    order.experienceReviewRequestSentOn = DateTimeOffset.FromUnixTimeSeconds(reviewRequest.sentOn).DateTime;
        //    await new OrderService(log).UpdateOrderAsync(order);
        //}

        //        private static async Task EnqueueProductReviewRequests(ILogger log, IBusinessReviewsService reviewService, Business business, BusinessOrder order, BusinessCustomer customer, List<BusinessProduct> businessProducts)
        //        {
        //            try
        //            {
        //                var queueManager = new QueueManager(Environment.GetEnvironmentVariable("ServiceBusConnection"), QueueName.ProductReviewRequest);

        //                /* There are multiple cases to handle:
        //                 * 1. Business did not send a scheduledDeliveryDate for one or more products.
        //                 * 2. scheduledDeliveryDate is different for one or more products.
        //                 */

        //                foreach(var group in order.items.Where(oi => oi.scheduledDeliveryDate.HasValue).GroupBy(oi => oi.scheduledDeliveryDate.Value.Date))
        //                {
        //                    var deliveryDate = group.Key;

        //                    // Convert the utc datetime to customer's time zone and strip off the time.
        //                    var localDeliveryDate = new DateTimeOffset(deliveryDate).ToOffset(TimeSpan.FromMinutes(order.customer.timezoneoffset)).Date;

        //                    // Set the queue time to 8PM and 3 days after delivery then convert back to UTC
        //                    localDeliveryDate = localDeliveryDate.AddDays(3).AddHours(20).ToUniversalTime();

        //#if DEBUG
        //                    // Strictly for debugging purposes
        //                    localDeliveryDate = DateTime.UtcNow.AddSeconds(30);
        //#endif

        //                    var reviewRequest = await reviewService.GetProductReviewRequestsAsync(order.businessId, order.id, localDeliveryDate);

        //                    if (reviewRequest == null)
        //                    {
        //                        reviewRequest = new ReviewRequest(business.id)
        //                        {
        //                            businessId = business.id,
        //                            businessLogoUrl = business.logoUrl,
        //                            orderId = order.id,
        //                            businessOrderId = order.orderId,
        //                            customerId = customer.id,
        //                            userId = customer.userId,
        //                            type = ReviewType.BusinessProduct,
        //                            sentOn = 0,
        //                            sendOn = new DateTimeOffset(localDeliveryDate).ToUnixTimeSeconds(),
        //                            products = new List<ReviewRequestProduct>()
        //                        };

        //                        foreach (var orderItem in group)
        //                        {
        //                            var businessProduct = businessProducts.First(bp => bp.externalProductId == orderItem.productId);
        //                            var product = new ReviewRequestProduct
        //                            {
        //                                id = businessProduct.id,
        //                                // Can't set this because it hasn't been generated yet
        //                                // imagePath = businessProduct.images[0].thumbpath,
        //                                name = businessProduct.name
        //                            };
        //                            reviewRequest.products.Add(product);
        //                        }

        //                        var requestId = await reviewService.AddReviewRequestAsync(reviewRequest);
        //                        reviewRequest = await reviewService.GetReviewRequestAsync(requestId);
        //                    }
        //                    else
        //                    {
        //                        log.LogCritical("Business product(s) review request has already been queued. Business ID={0}, Order ID={1}, ReviewRequest ID={2}", order.businessId, order.id, reviewRequest.id);
        //                        continue;
        //                    }

        //                    queueManager.WithPayload(new QueueMessage(QueueMessageTypes.BusinessReviews.ProductReviewRequest, new { orderId = order.id }, reviewRequest.id, localDeliveryDate));
        //                }

        //                await queueManager.Enqueue();
        //            }
        //            catch (Exception ex)
        //            {
        //                log.LogError(ex, "Unable to queue product review requests for order {0}", order.id);
        //            }
        //        }

        //[FunctionName("SendEmail")]
        //public static async Task SendEmail([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "sendemail")] HttpRequest req, ILogger log)
        //{
        //    //var messages = new JObject{
        //    //    {
        //    //        "Messages", new JArray
        //    //        {
        //    //            new JObject
        //    //            {
        //    //                {
        //    //                    "From",
        //    //                    new JObject
        //    //                    {
        //    //                        { "Email", "developer@truerevue.org" },
        //    //                        { "Name", @"The Iron Factory" }
        //    //                    }
        //    //                },
        //    //                {
        //    //                    "To",
        //    //                    new JArray
        //    //                    {
        //    //                        new JObject
        //    //                        {
        //    //                            { "Email", "poncev@gmail.com" },
        //    //                            { "Name", "Rick" }
        //    //                        }
        //    //                    }
        //    //                },
        //    //                { "TemplateID", 855560 },
        //    //                { "TemplateLanguage", true },
        //    //                { "Subject", @"Thank you for your purchase!" },
        //    //                {
        //    //                    "Variables",
        //    //                    new JObject
        //    //                    {
        //    //                        { "firstName", "Rick" }
        //    //                    }
        //    //                }
        //    //            }
        //    //        }
        //    //    }
        //    //};
        //    try
        //    {
        //        //var response = await "https://api.mailjet.com/v3.1/send"
        //        //    .WithBasicAuth("bbfb36fe9d6d44ec2baef8ea73868960", "22586b7dc35ad8d5dbf136f01c3e11a4")
        //        //    .PostJsonAsync(messages);
        //        //log.LogInformation("Mailjet response: {0}", "response");

        //        Emailer emailer = new Emailer(Environment.GetEnvironmentVariable("MailjetPublicKey"), Environment.GetEnvironmentVariable("MailjetPrivateKey"), log);
        //        emailer.AddVar("firstName", "Johnny");
        //        var recipients = new List<EmailMessageAddress>
        //        {
        //            new EmailMessageAddress("poncev@gmail.com", "Tiny")
        //        };
        //        var response = await emailer.SendEmail(EmailTemplate.BusinessReviewRequest, "Thank you for your purchase!", recipients);
        //        log.LogInformation("Response: " + response.Messages[0].To[0].MessageID);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.LogError(ex, "Error with mailjet");
        //    }
        //}

        #endregion Private Methods
    }
}
