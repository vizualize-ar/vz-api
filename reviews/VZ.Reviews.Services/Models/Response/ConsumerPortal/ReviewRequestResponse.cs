using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using VZ.Reviews.Services.Models.Data;

namespace VZ.Reviews.Services.Models.Response.ConsumerPortal
{
    public class ReviewRequestResponse
    {
        public string id { get; set; }
        public string reviewId { get; set; }
        public string status { get; set; }
        public ReviewType type { get; set; }
        public string businessLogoUrl { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ReviewRequestProductResponse> products { get; set; }
    }

    public class ReviewRequestProductResponse
    {
        public string id;
        public string reviewId { get; set; }
        public string name;
        public string imageUrl;
    }
}
