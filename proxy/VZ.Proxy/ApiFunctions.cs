using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TrueRevue.Proxy
{
    public static class ApiFunctions
    {
        [FunctionName("Health")]
        public static IActionResult Health([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")]HttpRequest req, ILogger log)
        {
            return new OkResult();
        }

        //[FunctionName("KeepAliveTimer")]
        //public static void KeepAlive([TimerTrigger("0 */4 * * * *")] TimerInfo myTimer, ILogger log)
        //{
        //    log.LogTrace("keep warm");
        //}
    }
}
