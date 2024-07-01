using Newtonsoft.Json.Linq;

namespace VZ.Shared.EventSchemas.Reviews
{
    public class ReviewUpdatedEventData
    {
        public JObject Review { get; set; }
    }
}
