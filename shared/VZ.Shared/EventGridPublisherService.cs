using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Rest.Azure;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;

namespace VZ.Shared
{
    public interface IEventGridPublisherService
    {
        Task PostEventGridEventAsync<TPayload>(string type, string subject, TPayload payload);
    }

    public class EventGridPublisherService : IEventGridPublisherService
    {
        private ILogger _log;

        public EventGridPublisherService(ILogger log)
        {
            _log = log;
        }

        public async Task PostEventGridEventAsync<T>(string type, string subject, T payload)
        {
            // get the connection details for the Event Grid topic
            var topicEndpointUri = new Uri(Config.EventGrid.TopicEndpoint);
            var topicEndpointHostname = topicEndpointUri.Host;
            var topicKey = Config.EventGrid.TopicKey;
            var topicCredentials = new TopicCredentials(topicKey);

            // prepare the events for submission to Event Grid
            var events = new List<Microsoft.Azure.EventGrid.Models.EventGridEvent>
            {
                new Microsoft.Azure.EventGrid.Models.EventGridEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    EventType = type,
                    Subject = subject,
                    EventTime = DateTime.UtcNow,
                    Data = payload,
                    DataVersion = "1"
                }
            };

            HttpResponseMessage response = null;
#if !DEBUG
            // publish the events
            var client = new EventGridClient(topicCredentials);
            var publishResult = await client.PublishEventsWithHttpMessagesAsync(topicEndpointHostname, events);
            response = publishResult.Response;
#else
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(events);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, topicEndpointUri)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            HttpClient client = new HttpClient();
            response = await client.SendAsync(request);
#endif
            if (response.IsSuccessStatusCode == false)
            {
                _log.LogCritical($"Failed to publish message. EventType={type}, Payload={Newtonsoft.Json.JsonConvert.SerializeObject(payload)}, StatusCode={response.StatusCode}, ReasonPhrase={response.ReasonPhrase}, Response={await response.Content.ReadAsStringAsync()}");
            }
        }
    }
}
