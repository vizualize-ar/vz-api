using Flurl.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace VZ.Shared
{
    public class ApiHelper
    {
        private static ApiHelper _instance;
        public static ApiHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ApiHelper();
                }

                return _instance;
            }
        }

        public async Task<dynamic> GetUserAsync(int id)
        {
            string apiBaseUrl = Config.Api.BaseUrl;
            string apiToken = Config.Api.Token;

            string url = $"{apiBaseUrl}/users/{id}?token={apiToken}";
            var user = (JObject)CacheManager.Get(url);

            if (user == null)
            {
                user = await url.GetJsonAsync<JObject>();
                CacheManager.Set(url, user, TimeSpan.FromMinutes(30));
            }

            return user;
        }

        public async Task<dynamic> GetUserAsync(string auth0Id)
        {
            string apiBaseUrl = Config.Api.BaseUrl;
            string apiToken = Config.Api.Token;

            string url = $"{apiBaseUrl}/users/{auth0Id}?token={apiToken}";
            var user = (JObject)CacheManager.Get(url);

            if (user == null)
            {
                user = await url.GetJsonAsync<JObject>();
                CacheManager.Set(url, user, TimeSpan.FromMinutes(30));
            }

            return user;
        }

        public async Task<dynamic> GetOrderAsync(long orderId)
        {
            string apiBaseUrl = Config.Api.BaseUrl;
            string apiToken = Config.Api.Token;

            string url = $"{apiBaseUrl}/orders/{orderId}?token={apiToken}";
            var user = (JObject)CacheManager.Get(url);

            if (user == null)
            {
                user = await url.GetJsonAsync<JObject>();
                CacheManager.Set(url, user, TimeSpan.FromMinutes(30));
            }

            return user;
        }
    }
}
