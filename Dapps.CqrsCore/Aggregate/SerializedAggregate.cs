using System;
using System.Collections.Generic;
using System.Text;

namespace Dapps.CqrsCore.Aggregate
{
    /// <summary>
    /// Provides a serialization wrapper for aggregates so we can use Entity Framework for basic DAL operations.
    /// </summary>
    public class SerializedAggregate
    {
        public string Class { get; set; }
        public DateTimeOffset? Expires { get; set; }

        public Guid AggregateId { get; set; }

        public string Type { get; set; }

    }
}
