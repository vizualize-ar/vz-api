using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace VZ.Shared.Queueing
{
    //public interface IQueueManager
    //{
    //    Task Enqueue<T>(string queueMessageType, T payload, DateTime? scheduledEnqueueTimeUtc = null) where T : class;

    //    Task Enqueue(string queueMessageType, string payload, DateTime? scheduledEnqueueTimeUtc = null);

    //    /// <summary>
    //    /// Enqueues a message, optionally at a specific time in the future
    //    /// </summary>
    //    /// <param name="payload"></param>
    //    /// <param name="scheduledEnqueueTimeUtc"></param>
    //    /// <returns></returns>
    //    Task Enqueue(string queueMessageType, byte[] payload, DateTime? scheduledEnqueueTimeUtc = null);
    //}

    public class QueueManager //: IQueueManager
    {
        //const string ServiceBusConnectionString = "Endpoint=sb://tr-api-qa.servicebus.windows.net/;SharedAccessKeyName=default;SharedAccessKey=3TIm48YXI+EhUphFJ+7Cr8NcPqhmUiYhaYwVOM+yMcc=;";
        //const string QueueName = "product-reviews";
        IQueueClient _queueClient;
        //List<Tuple<byte[], DateTime?>> _payloads = new List<Tuple<byte[], DateTime?>>();
        List<QueueMessage> _payloads = new List<QueueMessage>();

        public QueueManager(string serviceBusConnection, string queueName)
        {
            _queueClient = new QueueClient(serviceBusConnection, queueName);
            //_payloads = new List<Tuple<byte[], DateTime?>>();
        }

        /// <summary>
        /// Queue up the payload for enqueueing in ServiceBus. You must call Enqueue() for this to send the payload to ServiceBus
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="payload"></param>
        /// <param name="scheduledEnqueueTimeUtc"></param>
        /// <returns></returns>
        public QueueManager WithPayload<T>(string queueMessageType, T payload, DateTime? scheduledEnqueueTimeUtc = null)
        {
            //var payloadString = JsonConvert.SerializeObject(payload);
            //return WithPayload(new QueueMessage(payloadString, null, scheduledEnqueueTimeUtc));
            return WithPayload(new QueueMessage(queueMessageType, payload, null, scheduledEnqueueTimeUtc));
        }

        public QueueManager WithPayload(string queueMessageType, string payload, DateTime? scheduledEnqueueTimeUtc = null)
        {
            //return WithPayload(Encoding.UTF8.GetBytes(payload), scheduledEnqueueTimeUtc);
            return WithPayload(new QueueMessage(queueMessageType, payload, null, scheduledEnqueueTimeUtc));
        }

        public QueueManager WithPayload(string queueMessageType, byte[] payload, DateTime? scheduledEnqueueTimeUtc = null)
        {
            //this._payloads.Add(Tuple.Create(payload, scheduledEnqueueTimeUtc));
            return WithPayload(new QueueMessage(queueMessageType, payload, null, scheduledEnqueueTimeUtc));
        }

        public QueueManager WithPayload(QueueMessage message)
        {
            this._payloads.Add(message);
            return this;
        }

        public async Task Enqueue()
        {
            if (_payloads.Count == 0) return;

            //await Enqueue(this._payloads.ToArray());
            var messages = new List<Message>();
            foreach (var queueMessage in _payloads)
            {
                // Create a new message to send to the queue.
                var message = new Message(queueMessage.Payload);
                if (!String.IsNullOrWhiteSpace(queueMessage.Id))
                {
                    message.MessageId = queueMessage.Id;
                }
                if (queueMessage.ScheduledEnqueueTimeUtc.HasValue)
                {
                    message.ScheduledEnqueueTimeUtc = queueMessage.ScheduledEnqueueTimeUtc.Value;
                }
                messages.Add(message);
            }

            await _queueClient.SendAsync(messages);
            this._payloads.Clear();
        }

        public async Task Enqueue<T>(string queueMessageType, T payload, DateTime? scheduledEnqueueTimeUtc = null) where T : class
        {
            var payloadString = JsonConvert.SerializeObject(payload);
            await Enqueue(queueMessageType, Encoding.UTF8.GetBytes(payloadString), scheduledEnqueueTimeUtc);
        }

        public async Task Enqueue(string queueMessageType, string payload, DateTime? scheduledEnqueueTimeUtc = null)
        {
            await Enqueue(queueMessageType, Encoding.UTF8.GetBytes(payload), scheduledEnqueueTimeUtc);
        }

        public async Task Enqueue(string queueMessageType, byte[] payload, DateTime? scheduledEnqueueTimeUtc = null)
        {
            //// Create a new message to send to the queue.
            //var message = new Message(payload);
            //if (scheduledEnqueueTimeUtc.HasValue)
            //{
            //    message.ScheduledEnqueueTimeUtc = scheduledEnqueueTimeUtc.Value;
            //}

            //await _queueClient.SendAsync(message);
            this.WithPayload(queueMessageType, payload, scheduledEnqueueTimeUtc);
            await this.Enqueue();
        }

        public async Task Enqueue<T>(string queueMessageType, params Tuple<T, DateTime?>[] payloads)
        {
            foreach (var payload in payloads)
            {
                this._payloads.Add(new QueueMessage(queueMessageType, payload.Item1, payload.Item2));
            }
            await this.Enqueue();
        }

        public async Task Enqueue(string queueMessageType, params Tuple<string, DateTime?>[] payloads)
        {
            foreach (var payload in payloads)
            {
                this._payloads.Add(new QueueMessage(queueMessageType, payload.Item1, payload.Item2));
            }
            await this.Enqueue();
        }
    }
}
