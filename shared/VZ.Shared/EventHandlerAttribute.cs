using System;
using System.Collections.Generic;
using System.Text;

namespace VZ.Shared
{
    /// <summary>
    /// Decorator attribute for easy lookup of which functions handle which events
    /// </summary>
    public class EventHandlerAttribute : Attribute
    {
        public EventHandlerAttribute(string handlesEventType) { }
    }
}
