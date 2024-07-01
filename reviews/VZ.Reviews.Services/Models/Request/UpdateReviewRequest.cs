using Newtonsoft.Json;

namespace VZ.Reviews.Services.Models.Request
{
    public class UpdateReviewRequest
    {
        [JsonProperty("id")]
        public string Id;
        
        [JsonProperty("name")]
        public string Name;
    }
}