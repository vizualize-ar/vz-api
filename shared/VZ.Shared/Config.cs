using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace VZ.Shared
{
    public static class Config
    {
        private static ConfigHashtable _config;

        static Config()
        {
            _config = new ConfigHashtable((Hashtable)Environment.GetEnvironmentVariables());
            if (String.IsNullOrEmpty(Environment.GetEnvironmentVariable("FUNCTIONS_WORKER_RUNTIME")))
            {
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                var appSetting = new ConfigurationBuilder()
                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile($"appsettings.json", true)
                    .AddJsonFile($"appsettings.{env}.json")
                    .Build();
                _config.Merge(appSetting.AsEnumerable());
            }
        }

        public static string ConsumerPortalUrl => _config["ConsumerPortalUrl"];
        public static string ServiceBusConnection => _config["ServiceBusConnection"];
        public static int ThumbnailWidth => int.Parse(_config["ThumbnailWidth"]);
        public static string GoogleApplicationCredentials => _config["GoogleApplicationCredentials"];
        public static string IpfsApiUrl => _config["IpfsApiUrl"];
        public static bool IsDevelopment => string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "development", StringComparison.InvariantCultureIgnoreCase);

        public static class Api
        {
            public static string BaseUrl => _config["ApiBaseUrl"].ToString();
            public static string Token => _config["ApiToken"].ToString();
        }

        public static class Auth0
        {
            // public static string Domain => _config["Auth0Domain"].ToString();
            // public static string ClientId => _config["Auth0ClientID"].ToString();
            // public static string Certificate => _config["Auth0SigningCertificate"].ToString();
            public static string ConsumerPortalClientID => _config["Auth0.ConsumerPortalClientID"].ToString();

            public static class BusinessPortal
            {
                public static string Certificate => _config["Auth0.BusinessPortal.Certificate"].ToString();
                public static string ClientId => _config["Auth0.BusinessPortal.ClientId"].ToString();
                public static string ClientSecret => _config["Auth0.BusinessPortal.ClientSecret"].ToString();
                public static string Domain => _config["Auth0.BusinessPortal.Domain"].ToString();

                public static string ManagementClientId => _config["Auth0.BusinessPortal.ManagementClientId"].ToString();
                public static string ManagementClientSecret => _config["Auth0.BusinessPortal.ManagementClientSecret"].ToString();
            }
        }

        public static class Blob
        {
            public static string ConnectionString => _config["BlobConnectionString"];
            public static string BusinessMediaContainer => _config["BlobBusinessMediaContainer"];
            public static string BusinessProductMediaContainer => _config["BlobBusinessProductMediaContainer"];
            public static string ReviewRequestMediaContainer => _config["BlobReviewRequestMediaContainer"];
            public static string ReviewMediaContainer => _config["BlobReviewMediaContainer"];
        }

        public static class CDN
        {
            public static string BusinessImages => _config["CDN.BusinessImages"];
            public static string BusinessProductImages => _config["CDN.BusinessProductImages"];
            public static string ReviewImages => _config["CDN.ReviewImages"];
        }

        public static class DB
        {
            public static string PostgresConnection => _config["PostgresConnection"];
        }

        public static class Email
        {
            public static string FromEmail => _config["Email.FromEmail"];
            public static string FromName => _config["Email.FromName"];
            public static string PublicKey => _config["Email.PublicKey"];
            public static string PrivateKey => _config["Email.PrivateKey"];

            public static class Template
            {
                public static int BusinessReviewRequest => _config["Email.Template.BusinessReviewRequest", -1];
                public static int ProductReviewRequest => _config["Email.Template.ProductReviewRequest", -1];
            }
        }

        public static class Encryption
        {
            public static string Key => _config["Encryption.Key"];
            public static string IV => _config["Encryption.IV"];
        }

        public static class EventGrid
        {
            public static string TopicEndpoint => _config["EventGridTopicEndpoint"];
            public static string TopicKey => _config["EventGridTopicKey"];
        }

        class ConfigHashtable
        {
            private Hashtable _hashtable;

            public ConfigHashtable(Hashtable source)
            {
                _hashtable = source;
            }

            public string this[string name]
            {
                get
                {
                    var val = _hashtable[name];
                    if (val == null)
                    {
                        throw new ArgumentException("Setting not found: " + name);
                    }
                    return val.ToString();
                }
            }

            public int this[string name, int dummyDefault = -1]
            {
                get
                {
                    var val = _hashtable[name];
                    if (val == null)
                    {
                        throw new ArgumentException("Setting not found: " + name);
                    }
                    if (int.TryParse(val.ToString(), out int result)) return result;
                    throw new InvalidCastException("Setting could not be cast to int: " + name);
                }
            }

            public void Merge(IEnumerable<KeyValuePair<string, string>> hashtable)
            {
                foreach(var keyval in hashtable)
                {
                    _hashtable.Add(keyval.Key, keyval.Value);
                }
            }
        }
    }
}
