using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VZ.Shared.Email
{
    public class EmailMessages
    {
        public EmailMessages()
        {
            Messages = new List<EmailMessage>();
        }

        public List<EmailMessage> Messages { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? SandboxMode { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}
