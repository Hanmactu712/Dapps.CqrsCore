using Dapps.CqrsCore.Aggregate;
using Microsoft.Extensions.Configuration;
using System;

namespace Dapps.CqrsCore.Snapshots
{
    /// <summary>
    /// Implements the default snapshot strategy. A snapshot of an aggregate is taken after every Interval events.
    /// </summary>
    public class SnapshotStrategy : ISnapshotStrategy
    {
        private readonly int _interval;

        /// <summary>
        /// Constructs a new strategy.
        /// </summary>
        public SnapshotStrategy(IConfiguration configuration)
        {
            var config = configuration.GetSection("SnapshotStrategy:Interval").Value;
            _interval = !string.IsNullOrEmpty(config) ? Convert.ToInt16(config) : 100;
        }

        /// <summary>
        /// Returns true if a snapshot should be taken for the aggregate.
        /// </summary>
        public bool ShouldTakeSnapShot(AggregateRoot aggregate)
        {
            var i = aggregate.Version;
            for (var j = 0; j < aggregate.GetUncommittedChanges().Count; j++)
                if (++i % _interval == 0 && i != 0)
                    return true;
            return false;
        }
    }
}
