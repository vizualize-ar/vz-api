using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace VZ.Shared.UserAuthentication.Auth0
{
    public interface IAuth0Authenticator
    {
        (ClaimsPrincipal User, JwtSecurityToken ValidatedToken) AuthenticateAsync(string token, CancellationToken cancellationToken = default(CancellationToken));
    }
}