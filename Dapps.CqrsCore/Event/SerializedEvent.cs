using System;

namespace Dapps.CqrsCore.Event
{
    /// <summary>
    /// Event entity data which is persisted in the event store
    /// </summary>
    public class SerializedEvent
    {
        /// <summary>
        /// Aggregate unique id
        /// </summary>
        public Guid AggregateId { get; set; }
        /// <summary>
        /// Event version
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// User id who fired the event
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Class name of event
        /// </summary>
        public string Class { get; set; }
        /// <summary>
        /// Type of event
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Data which is carried by event
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// tracking source where event is fired such as command id
        /// </summary>
        public Guid ReferenceId { get; set; }
        /// <summary>
        /// timestamp of event firing
        /// </summary>
        public DateTimeOffset Time { get; set; }
    }
}
