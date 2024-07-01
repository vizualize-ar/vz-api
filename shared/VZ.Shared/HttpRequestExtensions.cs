using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace VZ.Shared
{
    public static class HttpRequestExtensions
    {
        public static string GetContinuationToken(this HttpRequest request)
        {
            return request.Headers["x-tr-continuation"];
        }

        public static void SetContinuationToken(this HttpRequest request, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }
            request.HttpContext.Response.Headers["x-tr-continuation"] = token;
        }

        // Would be used if CORS was handled by the Function, but we're using a proxy instead and that handles it.
        //public static void SetAllowedHeaders(this HttpRequest request)
        //{
        //    request.HttpContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
        //    request.HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "authorization, content-type, x-tr-continuation";
        //    request.HttpContext.Response.Headers["Access-Control-Expose-Headers"] = "x-tr-continuation";
        //}

        public static JObject GetSummary(this HttpRequest request)
        {
            dynamic summary = new JObject();
            SetHeader(request, summary, "user-agent", "ua");
            SetHeader(request, summary, "referer", "ref");
            SetHeader(request, summary, "accept-language", "lang");
            SetHeader(request, summary, "x-forwarded-for", "fwdFor");
            SetHeader(request, summary, "x-forwarded-host", "fwdHost");
            SetHeader(request, summary, "x-forwarded-server", "fwdServer");
            SetHeader(request, summary, "x-wap-profile", "wapPrId");
            SetHeader(request, summary, "x-wap-profile-dif", "wapPrDif");
            SetHeader(request, summary, "x-requested-with", "reqWith");

            summary.remoteIp = request.HttpContext.Connection.RemoteIpAddress.ToString();

            return summary;
        }

        private static void SetHeader(HttpRequest request, dynamic summary, string headerName, string fieldName)
        {
            string headerValue = request.Headers[headerName].ToString();
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                summary[fieldName] = headerValue;
            }
        }
    }
}
