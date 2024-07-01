//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using VZ.Orders.Services;
//using VZ.Shared;
//using VZ.Shared.Email;
//using VZ.Membership.Services;
//// using VZ.Reviews.Services;
//using Newtonsoft.Json.Linq;
//// using VZ.Reviews.Services.Models.Data;

//namespace VZ.WorkerApi
//{
//    public class WorkerService
//    {
//        ILogger _logger;

//        public WorkerService(ILogger log)
//        {
//            _logger = log;
//        }

//        public async Task ProcessProductReviewRequestMessage(long orderId, string reviewRequestId)
//        {
//            var orderService = new OrderService(_logger);
//            var businessService = new BusinessService();
//            var reviewService = new BusinessReviewsService(_logger);

//            var order = await orderService.GetOrderAsync(orderId);
//            var business = await businessService.GetBusinessAsync(order.businessId);
//            var reviewRequest = await reviewService.GetReviewRequestAsync(reviewRequestId);

//            await EnsureProductsHaveThumbnails(reviewRequest, reviewService);

//            Emailer emailer = new Emailer(Config.Email.PublicKey, Config.Email.PrivateKey, Config.Email.FromEmail, Config.Email.FromName, _logger);
//            emailer
//                .AddVar("businessLogoUrl", business.logoUrl)
//                .AddVar("businessName", business.name)
//                .AddVar("firstName", order.customer.firstName)
//                .AddVar("reviewRequestId", reviewRequest.id)
//                .AddVar("reviewRequestType", (int)reviewRequest.type)
//                .AddVar("reviewRequestUrl", $"{Config.ConsumerPortalUrl}/business/submit-review");

//            var products = new JArray();
//            foreach (var product in reviewRequest.products)
//            {
//                dynamic prod = new JObject();
//                prod.name = product.name;
//                prod.imageUrl = Shared.Config.CDN.BusinessProductImages + product.imagePath;
//                prod.id = product.id;
//                products.Add(prod);
//            }
//            //emailer.AddVar("products", products.ToString(Newtonsoft.Json.Formatting.None));
//            emailer.AddVar("products", products);

//            var recipients = new List<EmailMessageAddress>
//                {
//                    new EmailMessageAddress(order.customer.email, order.customer.firstName)
//                };
//            var response = await emailer.SendEmail(Config.Email.Template.ProductReviewRequest, "We would like your feedback", recipients);

//            reviewRequest.sentOn = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
//            reviewRequest.emailMessageId = response.Messages[0].To[0].MessageID;
//            await reviewService.UpdateReviewRequestAsync(reviewRequest);

//            // Update BusinessOrder.item
//            order.productReviewRequestSentOn = DateTimeOffset.FromUnixTimeSeconds(reviewRequest.sentOn).DateTime;
//            await orderService.UpdateOrderAsync(order);
//        }

//        private async Task EnsureProductsHaveThumbnails(ReviewRequest reviewRequest, BusinessReviewsService reviewService)
//        {
//            // Ensure all reviewRequest products have an imagePath
//            var businessProductService = new BusinessProductService(_logger);
//            var hasUpdate = false;
//            foreach (var product in reviewRequest.products.Where(p => p.imagePath == null))
//            {
//                var productImages = await businessProductService.GetThumbnailImages(product.id);
//                product.imagePath = productImages[0];
//                hasUpdate = true;
//            }
//            if (hasUpdate)
//            {
//                await reviewService.UpdateReviewRequestAsync(reviewRequest);
//            }
//        }
//    }
//}
