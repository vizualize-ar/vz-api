using System;
using VZ.Reviews.Services.Models.Response;
using Newtonsoft.Json;

namespace VZ.Reviews.Services.Converters
{
    //public class CategorySummariesConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        return (objectType == typeof(ReviewSummaries));
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        writer.WriteStartObject();
    //        foreach (var summary in (ReviewSummaries)value)
    //        {
    //            writer.WritePropertyName(summary.Id);
    //            summary.Id = null;
    //            serializer.Serialize(writer, summary);
    //        }
    //        writer.WriteEndObject();
    //    }
        
    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}