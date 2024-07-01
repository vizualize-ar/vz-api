using System;
using System.Collections.Generic;
using System.Text;
using Dawn;

namespace VZ.Shared.Email
{
    public class EmailMessage
    {
        public EmailMessageAddress From { get; set; }
        public List<EmailMessageAddress> To { get; set; }
        public int? TemplateId { get; set; }
        public bool TemplateLanguage { get; set; }
        public string Subject { get; set; }
        public Dictionary<string, object> Variables { get; set; }

        public bool TemplateErrorDeliver { get { return true; } }
        public EmailMessageAddress TemplateErrorReporting = new EmailMessageAddress("developer@truerevue.org", "Developer");

        public EmailMessage()
        {
            To = new List<EmailMessageAddress>();
        }
    }

    public class EmailMessageAddress
    {
        public string Email { get; set; }
        public string Name { get; set; }

        public EmailMessageAddress(string email, string name)
        {
            Guard.Argument(email).NotEmpty().NotNull();
            Guard.Argument(name).NotEmpty().NotNull();

            this.Email = email;
            this.Name = name;
        }
    }
}
