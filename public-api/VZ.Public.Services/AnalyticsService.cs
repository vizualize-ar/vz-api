using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VZ.Membership.Services.Models.Data;
using VZ.Shared;
using VZ.Shared.EventSchemas.Products;
using VZ.Shared.Data;

namespace VZ.Public.Services
{
    public class AnalyticsService
    {
        protected IEventGridPublisherService eventGridPublisherService;
        protected ILogger logger;

        public AnalyticsService(ILogger logger) : this(logger, new EventGridPublisherService(logger))
        { }

        public AnalyticsService(ILogger logger, IEventGridPublisherService eventGridPublisherService)
        {
            this.logger = logger;
            this.eventGridPublisherService = eventGridPublisherService;
        }

        public async Task LogProductViewed(JObject requestSummary, BusinessProduct businessProduct)
        {
            try
            {
                // post a ReviewCreated event to Event Grid
                var eventData = new ProductViewedEventData
                {
                    businessId = businessProduct.BusinessId,
                    productId = businessProduct.Id,
                    requestSummary = requestSummary
                };

                var subject = $"product_viewed/{businessProduct.Id}";
                await eventGridPublisherService.PostEventGridEventAsync(EventTypes.Products.ProductViewed, subject, eventData);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unable to log product viewed. businessId: {0}, productId: {1}", businessProduct?.BusinessId, businessProduct?.Id);
            }
        }
    }
}
