using Flurl.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json.Linq;

namespace VZ.Shared.ApiAuthentication
{
    public interface IApiAuthenticationService
    {
        /// <summary>
        /// Authenticates an API using the HttpRequest's Authorization header
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Tuple with bool indicating if the request is authenticated and a string which is the business ID</returns>
        bool Authenticate(HttpRequest request, out string businessId, ILogger log);
    }

    public class ApiAuthenticationService : IApiAuthenticationService
    {
        string _apiUrl;

        public ApiAuthenticationService(string apiBaseUrl)
        {
            this._apiUrl = apiBaseUrl;
        }

        //public bool Authenticate(HttpRequest request, out string businessId)
        //{
        //    businessId = null;
        //    string apiToken = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        //    string host = new Uri(request.Headers["Referer"]).DnsSafeHost;

        //    var authApiUrl = $"{_apiUrl}/businesses/auth/{apiToken}";
        //    var key = authApiUrl + host;
        //    AuthResult result = Shared.CacheManager.Get(key) as AuthResult;
        //    if (result == null)
        //    {
        //        WebClient webClient = new WebClient();
        //        webClient.Headers.Add("Referer", host);
        //        //result = authApiUrl.WithHeader("Referer", host).GetJsonAsync<AuthResult>().ConfigureAwait(false);
        //        result = JsonConvert.DeserializeObject<AuthResult>(webClient.DownloadString(authApiUrl));
        //        if (result == null)
        //        {
        //            return false;
        //        }
        //        Shared.CacheManager.Set(key, result);
        //    }
        //    businessId = result.businessId;
        //    return result.valid;
        //}

        public bool Authenticate(HttpRequest request, out string businessId, ILogger log)
        {
            try
            {
                businessId = null;
                string bearerToken = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                string encodedToken = BitConverter.ToString(Convert.FromBase64String(bearerToken)).Replace("-", "");
                var encryptor = new Security.Encryptor();
                var payloadString = encryptor.Decrypt(encodedToken);
                JObject payload = JObject.Parse(payloadString);
                if (payload.ContainsKey("b") == false)
                {
                    return false;
                }
                
                // TODO: Add customer host to payload and validate against request host

                businessId = payload["b"].Value<string>();
                return true;
            }
            catch(Exception ex)
            {
                businessId = null;
                log.LogError(ex, "Unable to authenticate API request");
                return false;
            }
        }

        class AuthResult
        {
            public bool valid { get; set; }
            public string businessId { get; set; }
        }
    }
}
