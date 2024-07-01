using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VZ.Shared.UserAuthentication
{
    public interface IHttpAuthenticationService
    {
        /// <summary>
        /// Authenticates a user using Authentication header and validates it as an auth0 JWT.
        /// </summary>
        /// <param name="req"></param>
        /// <returns>isAuthenticated</returns>
        bool Authenticate(HttpRequest req, out string auth0Id);

        /// <summary>
        /// Authenticates a user using Authentication header and validates it as an auth0 JWT.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="id">User ID from url path</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool Authenticate(HttpRequest req, string id, out string auth0Id);

        Tuple<bool, string> ExtractTokenFromHeader(HttpRequest req);
        Task<bool> Authorize(string auth0Id, string businessId);

        /// <summary>
        /// Authenticates and authorizes the user to the business
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        bool AuthenticateForBusiness(HttpRequest req, string businessId, out string auth0Id);
    }
}