using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VZ.Shared.EventSchemas.Audio;
using VZ.Shared.EventSchemas.Reviews;
using VZ.Shared.EventSchemas.Images;
using VZ.Shared.EventSchemas.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VZ.Shared
{
    public interface IEventGridSubscriberService
    {
        IActionResult HandleSubscriptionValidationEvent(HttpRequest req);
        bool HandleSubscriptionValidationEvent(HttpRequest req, out IActionResult result);
        (EventGridEvent eventGridEvent, string userId, string itemId) DeconstructEventGridMessage(HttpRequest req);
    }

    public class EventGridSubscriberService : IEventGridSubscriberService
    {
        internal const string EventGridSubscriptionValidationHeaderKey = "Aeg-Event-Type";

        public IActionResult HandleSubscriptionValidationEvent(HttpRequest req)
        {
            if (req.Body.CanSeek)
            {
                req.Body.Position = 0;
            }

            var requestBody = new StreamReader(req.Body).ReadToEnd();
            if (string.IsNullOrEmpty(requestBody))
            {
                return null;
            }

            dynamic data = JsonConvert.DeserializeObject(requestBody);
            
            foreach (var dataEvent in data)
            {
                if (req.Headers.TryGetValue(EventGridSubscriptionValidationHeaderKey, out StringValues values) && values.Equals("SubscriptionValidation") &&
                    dataEvent.eventType == "Microsoft.EventGrid.SubscriptionValidationEvent")
                {
                    // this is a special event type that needs an echo response for Event Grid to work
                    var validationCode = dataEvent.data.validationCode; // TODO .ToString();
                    var echoResponse = new {validationResponse = validationCode};
                    return new Shared.JsonResult(echoResponse);
                }
            }

            return null;
        }

        public bool HandleSubscriptionValidationEvent(HttpRequest req, out IActionResult result)
        {
            result = HandleSubscriptionValidationEvent(req);
            return result != null;
        }

        public (EventGridEvent eventGridEvent, string userId, string itemId) DeconstructEventGridMessage(HttpRequest req)
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
                return (null, null, null);
            }
            if (eventGridEvents.Length > 1)
            {
                throw new InvalidOperationException("Expected only a single Event Grid event.");
            }
            var eventGridEvent = eventGridEvents.Single();

            // convert the 'data' property to a strongly typed object rather than a JObject
            eventGridEvent.Data = CreateStronglyTypedDataObject(eventGridEvent.Data, eventGridEvent.EventType);

            // find the user ID and item ID from the subject
            var eventGridEventSubjectComponents = eventGridEvent.Subject.Split('/');
            if (eventGridEventSubjectComponents.Length != 2)
            {
                throw new InvalidOperationException("Event Grid event subject is not in expected format.");
            }
            var userId = eventGridEventSubjectComponents[0];
            var itemId = eventGridEventSubjectComponents[1];
            
            return (eventGridEvent, userId, itemId);
        }

        private object CreateStronglyTypedDataObject(object data, string eventType)
        {
            switch (eventType)
            {
                // creates

                case EventTypes.Audio.AudioCreated:
                    return ConvertDataObjectToType<AudioCreatedEventData>(data);

                // updates

                case EventTypes.Audio.AudioTranscriptUpdated:
                    return ConvertDataObjectToType<AudioTranscriptUpdatedEventData>(data);

                case EventTypes.Reviews.ReviewImageUpdated:
                    return ConvertDataObjectToType<ReviewImageUpdatedEventData>(data);

                case EventTypes.Reviews.ReviewItemsUpdated:
                    return ConvertDataObjectToType<ReviewItemsUpdatedEventData>(data);

                case EventTypes.Reviews.ReviewNameUpdated:
                    return ConvertDataObjectToType<ReviewNameUpdatedEventData>(data);

                case EventTypes.Reviews.ReviewSynonymsUpdated:
                    return ConvertDataObjectToType<ReviewSynonymsUpdatedEventData>(data);


                // deletes

                case EventTypes.Audio.AudioDeleted:
                    return ConvertDataObjectToType<AudioDeletedEventData>(data);

                case EventTypes.Reviews.ReviewDeleted:
                    return ConvertDataObjectToType<ReviewDeletedEventData>(data);

                default:
                    throw new ArgumentException($"Unexpected event type '{eventType}' in {nameof(CreateStronglyTypedDataObject)}");
            }
        }

        private T ConvertDataObjectToType<T>(object dataObject)
        {
            if (dataObject is JObject o)
            {
                return o.ToObject<T>();
            }

            return (T) dataObject;
        }
    }
}
