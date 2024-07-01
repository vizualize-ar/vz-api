using System;
using System.Text;
using Newtonsoft.Json;
using Dawn;

namespace VZ.Shared.Queueing
{
    public class QueueMessage
    {
        public string QueueMessageType { get; set; }
        public string Id { get; set; }
        public byte[] Payload { get; set; }
        public DateTime? ScheduledEnqueueTimeUtc { get; set; }

        public QueueMessage(string queueMessageType, object payload, DateTime? scheduledEnqueueTimeUtc = null) :
            this(queueMessageType, JsonConvert.SerializeObject(payload), null, scheduledEnqueueTimeUtc)
        { }

        public QueueMessage(string queueMessageType, string payload, DateTime? scheduledEnqueueTimeUtc = null) :
            this(queueMessageType, Encoding.UTF8.GetBytes(payload), null, scheduledEnqueueTimeUtc)
        { }

        public QueueMessage(string queueMessageType, byte[] payload, DateTime? scheduledEnqueueTimeUtc = null) :
            this(queueMessageType, payload, null, scheduledEnqueueTimeUtc)
        { }

        public QueueMessage(string queueMessageType, object payload, string id = null, DateTime? scheduledEnqueueTimeUtc = null) :
            this(queueMessageType, JsonConvert.SerializeObject(payload), id, scheduledEnqueueTimeUtc)
        { }

        public QueueMessage(string queueMessageType, string payload, string id = null, DateTime? scheduledEnqueueTimeUtc = null) : 
            this(queueMessageType, Encoding.UTF8.GetBytes(payload), id, scheduledEnqueueTimeUtc)
        { }

        public QueueMessage(string queueMessageType, byte[] payload, string id = null, DateTime? scheduledEnqueueTimeUtc = null)
        {
            Guard.Argument(queueMessageType).NotEmpty().NotNull();

            this.QueueMessageType = queueMessageType;
            this.Payload = payload;
            this.Id = id;
            this.ScheduledEnqueueTimeUtc = scheduledEnqueueTimeUtc;
        }
    }
}
