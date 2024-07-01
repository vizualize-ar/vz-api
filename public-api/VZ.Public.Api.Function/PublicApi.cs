using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VZ.Shared;
using VZ.Shared.ApiAuthentication;
using VZ.Membership.Services;
using VZ.Membership.Services.Models.Response.Public;
using VZ.Public.Services;

namespace VZ.Public.Api
{
    public static class PublicApi
    {
        private static IApiAuthenticationService _apiAuthenticationService = new ApiAuthenticationService(Config.Api.BaseUrl);

        /// <summary>
        /// This returns the 3d model and settings for a client website.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("P_GetProducts")]
        public static async Task<IActionResult> W_GetProducts(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "p/products/{productExternalId}")] HttpRequest req, ILogger log, string productExternalId)
        {
            try
            {
                if (!_apiAuthenticationService.Authenticate(req, out string businessId, log))
                {
                    return new UnauthorizedResult();
                }

                IBusinessProductService businessProductService = new BusinessProductService(log);
                var product = await businessProductService.GetByExternalIdAsync(productExternalId, Int64.Parse(businessId));
                if (product != null)
                {
                    var analyticsService = new AnalyticsService(log);
                    analyticsService.LogProductViewed(req.GetSummary(), product);

                    var response = new BusinessProductResponse(product);
                    return new Shared.JsonResult(response);
                }

                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unhandled exception");
                return new InternalServerErrorResult();
            }
        }
    }
}
