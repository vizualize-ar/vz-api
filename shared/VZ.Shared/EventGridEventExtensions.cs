using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VZ.Shared
{
    public static class EventGridEventExtensions
    {
        public static EventGridEvent<T> ToEvent<T>(this HttpRequest req)
        {
            // read the request stream
            if (req.Body.CanSeek)
            {
                req.Body.Position = 0;
            }
            var requestBody = new StreamReader(req.Body).ReadToEnd();

            // deserialise into a single Event Grid event - we won't allow multiple events to be processed
            var eventGridEvents = JsonConvert.DeserializeObject<EventGridEvent[]>(requestBody);
            if (eventGridEvents.Length == 0)
            {
                return null;
            }
            if (eventGridEvents.Length > 1)
            {
                throw new InvalidOperationException("Expected only a single Event Grid event.");
            }
            var eventGridEvent = eventGridEvents.Single();
            return new EventGridEvent<T>
            {
                EventType = eventGridEvent.EventType,
                Id = eventGridEvent.Id,
                EventTime = eventGridEvent.EventTime,
                Subject = eventGridEvent.Subject,
                Topic = eventGridEvent.Topic,
                Data = ConvertDataObjectToType<T>(eventGridEvent.Data)
            };
        }

        public static EventGridEvent<T> ToEvent<T>(this Microsoft.Azure.EventGrid.Models.EventGridEvent eventGridEvent)
        {
            return new EventGridEvent<T>
            {
                EventType = eventGridEvent.EventType,
                Id = eventGridEvent.Id,
                EventTime = eventGridEvent.EventTime,
                Subject = eventGridEvent.Subject,
                Topic = eventGridEvent.Topic,
                Data = ConvertDataObjectToType<T>(eventGridEvent.Data)
            };
        }

        private static T ConvertDataObjectToType<T>(object dataObject)
        {
            if (dataObject is JObject o)
            {
                return o.ToObject<T>();
            }

            return (T)dataObject;
        }
    }
}
