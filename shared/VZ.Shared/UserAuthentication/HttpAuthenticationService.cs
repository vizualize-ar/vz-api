using Dawn;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VZ.Shared.UserAuthentication
{
    using Auth0;
    using Flurl.Http;
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;

    public class HttpAuthenticationService  : IHttpAuthenticationService
    {
        private IAuth0Authenticator _auth0Authenticator;
        public HttpAuthenticationService(string auth0Domain , string auth0ClientID, string auth0SigningCertificate)
        {
            Guard.Argument(auth0Domain).NotEmpty().NotNull();
            Guard.Argument(auth0ClientID).NotEmpty().NotNull();
            Guard.Argument(auth0SigningCertificate).NotEmpty().NotNull();

            _auth0Authenticator = new Auth0Authenticator(auth0Domain, new List<string> { auth0ClientID }, auth0SigningCertificate);
        }

        public HttpAuthenticationService(IAuth0Authenticator auth0Authenticator)
        {
            _auth0Authenticator = auth0Authenticator;
        }

        /// <summary>
        /// Authenticates a user using Authentication header and validates it as an auth0 JWT.
        /// </summary>
        /// <param name="req"></param>
        /// <returns>{ isAuthenticated, username }</returns>
        public bool Authenticate(HttpRequest req, out string auth0Id)
        {
            auth0Id = "";
            var response = ExtractTokenFromHeader(req);
            if (response.Item1 == false)
            {
                return false;
            }
            var tokenString = response.Item2;
            var result = _auth0Authenticator.AuthenticateAsync(tokenString);
            if (result.User == null)
            {
                return false;
            }
            auth0Id = result.ValidatedToken.Subject.Replace("auth0|", "");
            return true;
        }

        public bool Authenticate(HttpRequest req, string id, out string userId)
        {
            return 
                Authenticate(req, out userId) &&
                id.Equals(userId, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool AuthenticateForBusiness(HttpRequest req, string businessId, out string userId)
        {
            return Authenticate(req, out userId) && Authorize(userId, businessId).Result;
        }

        public async Task<bool> Authorize(string auth0Id, string businessId)
        {
            if (string.IsNullOrEmpty(auth0Id) || string.IsNullOrWhiteSpace(businessId)) return false;

            //string apiBaseUrl = Config.Api.BaseUrl;
            //string apiToken = Config.Api.Token;

            //string url = $"{apiBaseUrl}/users/{userId}?token={apiToken}";
            //var user = (JObject)CacheManager.Get(url);
            //if (user == null)
            //{
            //    user = await url.GetJsonAsync<JObject>();
            //    CacheManager.Set(url, user, TimeSpan.FromMinutes(30));
            //}
            var user = (JObject)await ApiHelper.Instance.GetUserAsync(auth0Id);
            if (Array.Exists(user["businessIds"].ToObject<string[]>(), id => id == businessId) == false)
            {
                return false;
            }
            return true;
        }

        public Tuple<bool, string> ExtractTokenFromHeader(HttpRequest req)
        {
            StringValues headerValues = new StringValues();
            if (!req.Headers.TryGetValue("Authorization", out headerValues))
            {
                return new Tuple<bool, string>(false, "Missing auth header in request");
            }
            var authToken = headerValues.FirstOrDefault();
            if (String.IsNullOrWhiteSpace(authToken))
            {
                return new Tuple<bool, string>(false, "Missing auth header in request");
            }
            var parts = authToken.Split(new char[] { ' ' });
            if (parts == null || parts.Length != 2)
            {
                return new Tuple<bool, string>(false, "auth header is not well formed");
            }
            var tokenString = parts[1];
            return new Tuple<bool, string>(true, tokenString);
        }
    }
}
