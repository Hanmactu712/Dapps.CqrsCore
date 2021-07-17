using System;
using System.IO;

namespace Dapps.CqrsCore.Event
{
    /// <summary>
    /// Options for command store
    /// </summary>
    public class EventStoreOptions
    {
        /// <summary>
        /// Location to save data of events when boxing & unboxing
        /// </summary>
        public string EventLocalStorage { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EventSourcing");
    }
}
