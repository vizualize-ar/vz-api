﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VZ.Reviews.Services.Models.Data
{
    public class ReviewItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ItemType Type { get; set; }

        [JsonProperty("preview")]
        public string Preview { get; set; }
    }
}