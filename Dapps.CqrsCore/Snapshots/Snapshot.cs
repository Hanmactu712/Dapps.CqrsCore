using System;

namespace Dapps.CqrsCore.Snapshots
{
    /// <summary>
    /// Represents a memento for a specific version of a specific aggregate.
    /// </summary>
    public class Snapshot
    {
        /// <summary>
        /// Aggregate Id
        /// </summary>
        public Guid AggregateId { get; set; }
        /// <summary>
        /// Version of event
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// State of event
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// time to take the snapshot
        /// </summary>
        public DateTimeOffset Time { get; set; }
    }
}
