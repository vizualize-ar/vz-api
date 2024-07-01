using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace VZ.Shared
{
    public class JsonResult : Microsoft.AspNetCore.Mvc.JsonResult
    {
        public JsonResult(object result) : base(result, Serialization.JsonConvert.SerializerSettings)
        {
        }
    }
}
