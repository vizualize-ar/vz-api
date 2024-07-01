using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VZ.Shared;
using VZ.Shared.ApiAuthentication;
using VZ.Membership.Services;
using VZ.Membership.Services.Models.Response.Public;
using VZ.Public.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace VZ.Public.Api.Controllers
{
    [Route("p/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        // private static IApiAuthenticationService _apiAuthenticationService = new ApiAuthenticationService(Config.Api.BaseUrl);
        private readonly IApiAuthenticationService apiAuthenticationService;
        private readonly ILogger<ProductController> log;

        public ProductController(ILogger<ProductController> log, IConfiguration configuration)
        {
            this.log = log;
            this.apiAuthenticationService = new ApiAuthenticationService(configuration.GetValue<string>("ApiBaseUrl"));
        }

        [HttpGet("{productExternalId}")]
        public async Task<IActionResult> GetProduct(string productExternalId)
        {
            try
            {
                if (!apiAuthenticationService.Authenticate(this.Request, out string businessId, log))
                {
                    return new UnauthorizedResult();
                }

                IBusinessProductService businessProductService = new BusinessProductService(log);
                var product = await businessProductService.GetByExternalIdAsync(productExternalId, Int64.Parse(businessId));
                if (product != null)
                {
                    var analyticsService = new AnalyticsService(log);
                    analyticsService.LogProductViewed(this.Request.GetSummary(), product);

                    var response = new BusinessProductResponse(product);
                    return new Shared.JsonResult(response);
                }

                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unhandled exception");
                return new EmptyResult();
            }
        }
    }
}
