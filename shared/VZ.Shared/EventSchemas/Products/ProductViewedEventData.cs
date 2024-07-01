using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Shared.EventSchemas.Products
{
    public class ProductViewedEventData
    {
        public long businessId { get; set; }
        public long productId { get; set; }
        public JObject requestSummary { get; set; }
    }
}
