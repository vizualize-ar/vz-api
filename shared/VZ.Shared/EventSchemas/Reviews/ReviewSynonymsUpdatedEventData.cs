using System.Collections.Generic;

namespace VZ.Shared.EventSchemas.Reviews
{
    public class ReviewSynonymsUpdatedEventData
    {
        public string Name { get; set; }
        public IEnumerable<string> Synonyms { get; set; }
    }
}