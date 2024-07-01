using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VZ.Shared.UserAuthentication.Auth0
{
    public sealed class Auth0Authenticator : IAuth0Authenticator
    {
        private readonly TokenValidationParameters _parameters;
        private readonly JwtSecurityTokenHandler _handler;

        /// <summary>
        /// Creates a new authenticator. In most cases, you should only have one authenticator instance in your application.
        /// </summary>
        /// <param name="auth0Domain">The domain of the Auth0 account, e.g., <c>"myauth0test.auth0.com"</c>.</param>
        /// <param name="audiences">The valid audiences for tokens. This must include the "audience" of the access_token request, and may also include a "client id" to enable id_tokens from clients you own.</param>
        public Auth0Authenticator(string auth0Domain, IEnumerable<string> auth0ClientIDs, string auth0SigningCertificate)
        {
            var certificate = new X509Certificate2(Convert.FromBase64String(auth0SigningCertificate));

            // Create the parameters
            _parameters = new TokenValidationParameters()
            {
                RequireSignedTokens = true,
                ValidAudiences = auth0ClientIDs.ToArray(),
                ValidateAudience = true,
                ValidIssuer = $"https://{auth0Domain}/",
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = new X509SecurityKey(certificate),
                NameClaimType = "name"
            };
            _handler = new JwtSecurityTokenHandler();
        }

        /// <summary>
        /// Authenticates the user token. Returns a user principal containing claims from the token and a token that can be used to perform actions on behalf of the user.
        /// Throws an exception if the token fails to authenticate.
        /// This method has an asynchronous signature, but usually completes synchronously.
        /// </summary>
        /// <param name="token">The token, in JWT format.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        public (ClaimsPrincipal User, JwtSecurityToken ValidatedToken) AuthenticateAsync(string token, CancellationToken cancellationToken = new CancellationToken())
        {
#if DEBUG
            IdentityModelEventSource.ShowPII = true;
#endif
            // Validate the token
            try
            {
                var user = _handler.ValidateToken(token, _parameters, out var securityToken);
                return (user, (JwtSecurityToken)securityToken);
            }
            catch (SecurityTokenException)
            {
                return (null, null);
            }
        }
    }

    public static class Auth0AuthenticatorExtensions
    {
        /// <summary>
        /// Authenticates the user via an "Authentication: Bearer {token}" header.
        /// Returns a user principal containing claims from the token and a token that can be used to perform actions on behalf of the user.
        /// Throws an exception if the token fails to authenticate or if the Authentication header is malformed.
        /// This method has an asynchronous signature, but usually completes synchronously.
        /// </summary>
        /// <param name="this">The authenticator instance.</param>
        /// <param name="header">The authentication header.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        public static (ClaimsPrincipal User, SecurityToken ValidatedToken) AuthenticateAsync(this Auth0Authenticator @this, AuthenticationHeaderValue header, CancellationToken cancellationToken = new CancellationToken())
        {
            if (header == null || !string.Equals(header.Scheme, "Bearer", StringComparison.InvariantCultureIgnoreCase))
                throw new InvalidOperationException("Authentication header does not use Bearer token.");
            return @this.AuthenticateAsync(header.Parameter, cancellationToken);
        }

        /// <summary>
        /// Authenticates the user via an "Authentication: Bearer {token}" header in an HTTP request message.
        /// Returns a user principal containing claims from the token and a token that can be used to perform actions on behalf of the user.
        /// Throws an exception if the token fails to authenticate or if the Authentication header is missing or malformed.
        /// This method has an asynchronous signature, but usually completes synchronously.
        /// </summary>
        /// <param name="this">The authenticator instance.</param>
        /// <param name="request">The HTTP request.</param>
        /// <param name="cancellationToken">An optional cancellation token.</param>
        public static (ClaimsPrincipal User, SecurityToken ValidatedToken) AuthenticateAsync(this Auth0Authenticator @this, HttpRequestMessage request, CancellationToken cancellationToken = new CancellationToken())
            => @this.AuthenticateAsync(request.Headers.Authorization, cancellationToken);
    }
}
