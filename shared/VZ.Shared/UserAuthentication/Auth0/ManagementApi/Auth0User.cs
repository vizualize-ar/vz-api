using System;
using Newtonsoft.Json.Linq;

namespace VZ.Shared.UserAuthentication.Auth0.ManagementApi
{
    public class Auth0User
    {
        public string email { get; set; }
        public bool email_verified { get; set; }
        public string username { get; set; }
        public string phone_number { get; set; }
        public bool phone_verified { get; set; }
        public string user_id { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public Auth0Identity[] identities { get; set; }
        public JObject app_metadata { get; set; }
        public JObject user_metadata { get; set; }
        public string picture { get; set; }
        public string name { get; set; }
        public string nickname { get; set; }
        public string last_ip { get; set; }
        public string last_login { get; set; }
        public int logins_count { get; set; }
        public bool blocked { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
    }

    public class Auth0Identity
    {
        public string connection { get; set; }
        public string user_id { get; set; }
        public string provider { get; set; }
        public bool isSocial { get; set; }
    }
}
