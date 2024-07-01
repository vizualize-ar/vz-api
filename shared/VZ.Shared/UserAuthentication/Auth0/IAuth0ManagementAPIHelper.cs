using System.Net.Http;
using System.Threading.Tasks;

namespace VZ.Shared.UserAuthentication.Auth0
{
    using ManagementApi;
    using Newtonsoft.Json.Linq;

    public interface IAuth0ManagementAPIHelper
    {
        Task<HttpResponseMessage> CreateAuth0User(string email, string password, string token);
        Task<HttpResponseMessage> GetManagementAPITokenAsync();

        Task<Auth0User> GetUserAsync(string auth0Id, string authToken);

        /// <summary>
        /// Updates one or all fields of a user. Specify which fields to update in pathObject
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patchObject"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<Auth0User> UpdateUser(string id, JObject patchObject, string token);
    }
}