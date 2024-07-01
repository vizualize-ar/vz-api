using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json;

namespace VZ.Shared.Email
{
    public class Emailer
    {
        private string _publicKey, _privateKey;
        private string _fromEmail, _fromName;
        private ILogger _log;
        private Dictionary<string, object> _variables = new Dictionary<string, object>();
        private bool? _sandbox;

        public Emailer(string publicKey, string privateKey, ILogger log, bool? sandbox = null)
            : this(publicKey, privateKey, "developer@vizualize.io", "Vizualize", log, sandbox)
        {
        }

        public Emailer(string publicKey, string privateKey, string fromEmail, string fromName, ILogger log, bool? sandbox = null)
        {
            _publicKey = publicKey;
            _privateKey = privateKey;
            _fromEmail = fromEmail;
            _fromName = fromName;
            _log = log;
            _sandbox = sandbox;
        }

        public Emailer AddVar(string key, string value)
        {
            _variables.Add(key, value);
            return this;
        }

        public Emailer AddVar(string key, object value)
        {
            _variables.Add(key, value);
            return this;
        }

        public async Task<Response> SendEmail(int templateId, string subject, string toEmail, string toName, string fromName = "Vizualize")
        {
            var recipients = new List<EmailMessageAddress>{
                new EmailMessageAddress(toEmail, toName)
            };
            return await SendEmail(templateId, subject, recipients, fromName);
        }

        public async Task<Response> SendEmail(int templateId, string subject, List<EmailMessageAddress> recipients, string fromEmail = null, string fromName = null)
        {
            var messages = new EmailMessages
            {
                Messages = new List<EmailMessage>
                {
                    new EmailMessage
                    {
                        From = new EmailMessageAddress(fromEmail ?? _fromEmail, fromName ?? _fromName),
                        To = recipients,
                        Subject = subject,
                        TemplateId = templateId,
                        TemplateLanguage = true,
                        Variables = _variables
                    }
                },
                SandboxMode = _sandbox
            };
            try
            {
                HttpResponseMessage httpResponse = await "https://api.mailjet.com/v3.1/send"
                    .WithBasicAuth(_publicKey, _privateKey)
                    .PostJsonAsync(messages);
                var response = JsonConvert.DeserializeObject<Response>(await httpResponse.Content.ReadAsStringAsync());
                return response;
            }
            catch (FlurlHttpException ex)
            {
                var response = await ex.GetResponseStringAsync();
                _log.LogError(ex, "Error with mailjet. Response: {0}", response);
                return null;
            }
        }

        public static async Task SendInternalEmail(string to, string subject, string body)
        {
            var smtpClient = new System.Net.Mail.SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential("team@vizualize.io", "zfkqxvkukdvfbtrm")
            };

            using (var message = new System.Net.Mail.MailMessage("team@vizualize.io", to)
            {
                Subject = subject,
                Body = body
            })
            {
                await smtpClient.SendMailAsync(message);
            }
        }
    }
}
