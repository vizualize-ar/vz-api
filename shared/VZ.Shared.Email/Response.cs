using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Shared.Email
{
    public class Response
    {
        public List<ResponseMessage> Messages { get; set; }
    }

    public class ResponseMessage
    {
        public string Status { get; set; }
        public string CustomID { get; set; }
        public List<ResponseMessageAddress> To { get; set; }
        public List<ResponseMessageAddress> Cc { get; set; }
        public List<ResponseMessageAddress> Bcc { get; set; }
    }

    public class ResponseMessageAddress
    {
        public string Email { get; set; }
        public string MessageUUID { get; set; }
        public string MessageID { get; set; }
        public string MessageHref { get; set; }
    }
}
