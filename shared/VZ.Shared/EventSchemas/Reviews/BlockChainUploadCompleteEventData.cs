using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Shared.EventSchemas.Reviews
{
    public class BlockchainUploadCompleteEventData
    {
        public UploadBlockchainEventData review { get; set; }
        public string transaction { get; set; }
    }
}
