using System;

namespace Dapps.CqrsCore.Event
{
    public interface IEvent
    {
        /// <summary>
        /// Unique Id of tied aggregate
        /// </summary>
        Guid AggregateId { get; set; }
        /// <summary>
        /// version of aggregate
        /// </summary>
        int Version { get; set; }
        //Guid UserId { get; set; }
        /// <summary>
        /// Timestamp of the event
        /// </summary>
        DateTimeOffset Time { get; set; }
        /// <summary>
        /// Reference to the source which event is fired. It may command Id or other source
        /// </summary>
        Guid ReferenceId { get; set; }
    }
}
