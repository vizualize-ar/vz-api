using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace VZ.Shared.Serialization
{
    //public static class Serializer
    //{
    //    /// <summary>
    //    /// Sets the global json.net serializer settings. This should be called in a functions static constructor.
    //    /// </summary>
    //    public static void Init()
    //    {
    //        // This doesn't seem to be working. See BusinessProduct.unit as an example.
    //        var settings = new JsonSerializerSettings
    //        {
    //            Converters = new List<JsonConverter> { new StringEnumConverter() }
    //        };
    //        JsonConvert.DefaultSettings = () => settings;
    //    }
    //}

    public static class JsonConvert
    {
        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            DateParseHandling = DateParseHandling.DateTimeOffset,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Converters = new List<JsonConverter> { new StringEnumConverter() },
            ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }
        };

        public static T DeserializeObject<T>(string value)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value, SerializerSettings);
        }
    }
}
