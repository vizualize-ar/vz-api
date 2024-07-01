using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VZ.Shared.UserAuthentication.Auth0
{
    using Auth0.ManagementApi;
    using Newtonsoft.Json.Linq;

    public class Auth0ManagementAPIHelper : IAuth0ManagementAPIHelper
    {
        private readonly string m_auth0baseURL;
        private readonly string m_connectionName;
        private readonly string m_clientID;
        private readonly string m_clientSecret;
        private readonly string m_audience;
        private static HttpClient m_httpClient;

        public Auth0ManagementAPIHelper(string auth0BaseURL, string connectionName, string clientID, string clientSecret, string audience)
        {
            m_clientID = clientID;
            m_clientSecret = clientSecret;
            m_audience = audience;
            m_auth0baseURL = auth0BaseURL;
            m_connectionName = connectionName;
            m_httpClient = new HttpClient();
            if (!auth0BaseURL.Contains("https:"))
            {
                auth0BaseURL = $"https://{auth0BaseURL}";
            }
            m_httpClient.BaseAddress = new Uri(auth0BaseURL);
        }

        public async Task<HttpResponseMessage> CreateAuth0User(string email, string password, string authToken)
        {
            var obj = new
            {
                connection = m_connectionName,
                email = email
            ,
                password = password
            };
            var json = JsonConvert.SerializeObject(obj);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/v2/users");
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
            httpRequest.Headers.Add("Authorization", $"Bearer {authToken}");
            var response = await m_httpClient.SendAsync(httpRequest);
            return response;
        }

        public async Task<Auth0User> GetUserAsync(string auth0Id, string authToken)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, "api/v2/users/auth0|" + auth0Id);
            httpRequest.Headers.Add("Authorization", $"Bearer {authToken}");
            try
            {
                var responseMessage = await m_httpClient.SendAsync(httpRequest);
                var responseString = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<Auth0User>(responseString);
                return response;
            }
            catch
            {
                return null;
            }
        }

        public async Task<HttpResponseMessage> GetManagementAPITokenAsync()
        {
            var requestDto = new
            {
                grant_type = "client_credentials",
                client_id = m_clientID,
                client_secret = m_clientSecret
                ,
                audience = m_audience
            };
            var json = JsonConvert.SerializeObject(requestDto);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "oauth/token");
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await m_httpClient.SendAsync(httpRequest);
            return response;
        }

        public async Task<Auth0User> UpdateUser(string id, JObject patchObject, string token)
        {
            //var client = new RestClient("https://YOUR_DOMAIN/api/v2/users/USER_ID");
            //var request = new RestRequest(Method.PATCH);
            //request.AddHeader("content-type", "application/json");
            //request.AddParameter("application/json", "{\"password\": \"NEW_PASSWORD\",\"connection\": \"Username-Password-Authentication\"}", ParameterType.RequestBody);
            //IRestResponse response = client.Execute(request);

            patchObject["connection"] = "Username-Password-Authentication";

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, "api/v2/users/auth0|" + id);
            httpRequest.Headers.Add("Authorization", $"Bearer {token}");
            httpRequest.Method = HttpMethod.Patch;
            // httpRequest.Content.Headers.Add("content-type", "application/json");
            httpRequest.Content = new StringContent(JsonConvert.SerializeObject(patchObject), Encoding.UTF8, "application/json");
            var responseMessage = await m_httpClient.SendAsync(httpRequest);
            var responseString = await responseMessage.Content.ReadAsStringAsync();
            if (responseMessage.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var respOjb = JObject.Parse(responseString);
                throw new ApplicationException(respOjb["message"].Value<string>());
            }
            var response = JsonConvert.DeserializeObject<Auth0User>(responseString);
            return response;
        }
    }
}