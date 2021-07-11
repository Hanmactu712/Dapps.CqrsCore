﻿using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Event;

namespace Dapps.CqrsCore.Snapshots
{
    /// <summary>
    /// Defines the strategy to use for determining when a snapshot should be taken.
    /// </summary>
    public interface ISnapshotStrategy
    {
        /// <summary>
        /// Returns true if a snapshot should be taken for the aggregate.
        /// </summary>
        bool ShouldTakeSnapShot(AggregateRoot aggregate);
    }
}
