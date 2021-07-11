using System;

namespace Dapps.CqrsCore.Snapshots
{
    /// <summary>
    /// Represents a memento for a specific version of a specific aggregate.
    /// </summary>
    public class Snapshot
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public string State { get; set; }
    }
}
