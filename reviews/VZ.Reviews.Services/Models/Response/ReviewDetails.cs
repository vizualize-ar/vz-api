using System.Collections.Generic;
using Newtonsoft.Json;

namespace VZ.Reviews.Services.Models.Response
{
    public class ReviewDetails
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty("synonyms")]
        public IList<string> Synonyms { get; set; }

        [JsonProperty("items")]
        public IList<ReviewItemDetails> Items { get; set; }
    }
}
