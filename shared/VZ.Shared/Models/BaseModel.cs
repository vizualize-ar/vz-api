using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Security.Cryptography;
using System.Text;

namespace VZ.Shared.Models
{
    public class BaseModel
    {
        /// <summary>
        /// Use this contructor to generate an id that contains the partition key as the suffix, dot delimited.
        /// Example: "abc123.xyz456"
        /// </summary>
        /// <param name="partitionKey"></param>
        public BaseModel(params string[] partitionKeys) : this()
        {
            this.NewId(partitionKeys);
        }

        public BaseModel()
        {
            this.NewId();
            createdOn = DateTime.UtcNow;
            updatedOn = DateTime.UtcNow;
        }

        public string id { get; set; }

        [JsonProperty("_ts")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime updatedOn { get; set; }

        // [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime createdOn { get; set; }

        [JsonProperty("_etag")]
        public string etag { get; set; }

        public void NewId(params string[] suffixIds)
        {
            this.id = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12);
            if (suffixIds != null && suffixIds.Length > 0)
            {
                for(int i = 0; i < suffixIds.Length; i++)
                {
                    this.id += "".IdDelimiter() + suffixIds[i];
                }
            }
        }
    }
}
