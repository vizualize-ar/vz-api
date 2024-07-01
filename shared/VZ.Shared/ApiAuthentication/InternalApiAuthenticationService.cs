using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Shared.ApiAuthentication
{
    public interface IInternalApiAuthenticationService
    {
        /// <summary>
        /// Authenticates an API using the HttpRequest's token query string parameter
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Tuple with bool indicating if the request is authenticated</returns>
        bool Authenticate(HttpRequest request, ILogger log);
    }

    public class InternalApiAuthenticationService : IInternalApiAuthenticationService
    {
        public bool Authenticate(HttpRequest request, ILogger log)
        {
            if (string.IsNullOrWhiteSpace(request.Query["token"]))
            {
                log.LogWarning("Internal API request is missing token. Path = {0}", request.Path);
                return false;
            }
            return request.Query["token"] == Config.Api.Token;
        }
    }
}
