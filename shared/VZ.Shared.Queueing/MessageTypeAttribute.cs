using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Shared
{
    /// <summary>
    /// Decorator attribute for easy lookup of which functions handle which queue message types
    /// </summary>
    public class QueueMessageHandlerAttribute : Attribute
    {
        public QueueMessageHandlerAttribute(string messageRelatesTo) { }
    }
}
