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
using VZ.Orders.Services;
using VZ.Orders.Services.Models.Request;
using VZ.Shared;
using VZ.Shared.ApiAuthentication;

namespace VZ.Public.Api
{
    public static partial class OrdersApiFunctions
    {
        private static IApiAuthenticationService _apiAuthenticationService = new ApiAuthenticationService(Config.Api.BaseUrl);

        [FunctionName("Widgets_AddBusinessOrder")]
        public static async Task<IActionResult> Widgets_AddBusinessOrder([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "w/orders")] HttpRequest req, ILogger log)
        {
            if (!_apiAuthenticationService.Authenticate(req, out string bid, log))
            {
                return new UnauthorizedResult();
            }
            if (!long.TryParse(bid, out long businessId))
            {
                return new BadRequestResult();
            }

            try
            {
                string requestBody = new StreamReader(req.Body).ReadToEnd();
                if (String.IsNullOrWhiteSpace(requestBody))
                {
                    return new BadRequestResult();
                }

                var order = JsonConvert.DeserializeObject<NewBusinessOrder>(requestBody);
                IOrderService _orderService = new OrderService(log);
                var orderId = await _orderService.AddOrderAsync(businessId, order);
                if (orderId == 0)
                {
                    log.LogCritical("Unable to save business order. BusinessId={0}, Request body:{1}", businessId, requestBody);
                    return new InternalServerErrorResult();
                }

                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unhandled exception: {0}", ex.Message);
                return new InternalServerErrorResult();
            }
        }
    }
}
