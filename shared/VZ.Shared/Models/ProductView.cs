using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Shared.Models
{
    public class ProductView : SqlBaseModel
    {
        public long businessId { get; set; }
        public long productId { get; set; }
        public JObject requestData { get; set; }
    }
}
