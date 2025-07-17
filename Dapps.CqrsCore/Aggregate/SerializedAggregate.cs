using System;

namespace Dapps.CqrsCore.Aggregate;

/// <summary>
/// Provides a serialization wrapper for aggregates so we can use Entity Framework for basic DAL operations.
/// </summary>
public class SerializedAggregate
{
    /// <summary>
    /// The class name of the aggregate.
    /// </summary>
    public string Class { get; set; }

    /// <summary>
    /// The Expires date of the aggregate.
    /// </summary>
    public DateTimeOffset? Expires { get; set; }

    /// <summary>
    /// The aggregate identifier.
    /// </summary>
    public Guid AggregateId { get; set; }

    /// <summary>
    /// The Class type of the aggregate.
    /// </summary>
    public string Type { get; set; }
}
