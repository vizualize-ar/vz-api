using Newtonsoft.Json;

namespace VZ.Reviews.Services.Models.Response
{
    public class ReviewSummary
    {
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
