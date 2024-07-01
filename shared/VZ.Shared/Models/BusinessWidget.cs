using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Shared.Models
{
    public class BusinessWidget : BaseModel
    {
        public BusinessWidget(string partitionKey) : base(partitionKey) { }

        public string businessId { get; set; }
        public WidgetType widgetType { get; set; }
        public decimal rating { get; set; }
        public string[] tags { get; set; }

        public string html { get; set; }
    }
}
