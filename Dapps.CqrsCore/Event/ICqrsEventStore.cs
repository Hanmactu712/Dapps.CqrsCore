using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dapps.CqrsCore.Event;

/// <summary>
/// interface of event store
/// </summary>
public interface ICqrsEventStore
{
    /// <summary>
    /// Utility for serializing and deserializing events.
    /// </summary>
    ICqrsSerializer Serializer { get; }

    /// <summary>
    /// Returns true if an aggregate exists.
    /// </summary>
    bool Exists(Guid aggregateId);

    /// <summary>
    /// Returns true if an aggregate exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid aggregateId, CancellationToken cancellation = default);

    /// <summary>
    /// Returns true if an aggregate with a specific version exists.
    /// </summary>
    bool Exists(Guid aggregateId, int version);

    /// <summary>
    /// Returns true if an aggregate with a specific version exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid aggregateId, int version, CancellationToken cancellation = default);

    /// <summary>
    /// Gets events for an aggregate starting at a specific version. To get all events use version = -1.
    /// </summary>
    IEnumerable<ICqrsEvent> Get(Guid aggregateId, int version);

    /// <summary>
    /// Gets events for an aggregate starting at a specific version. To get all events use version = -1.
    /// </summary>
    Task<IEnumerable<ICqrsEvent>> GetAsync(Guid aggregateId, int version, CancellationToken cancellation = default);

    /// <summary>
    /// Gets all aggregates that are scheduled to expire at (or before) a specific time on a specific date.
    /// </summary>
    IEnumerable<Guid> GetExpired(DateTimeOffset at);

    /// <summary>
    /// Gets all aggregates that are scheduled to expire at (or before) a specific time on a specific date.
    /// </summary>
    Task<IEnumerable<Guid>> GetExpiredAsync(DateTimeOffset at, CancellationToken cancellation = default);

    /// <summary>
    /// Save events.
    /// </summary>
    void Save(CqrsAggregateRoot aggregate, IEnumerable<ICqrsEvent> events);

    /// <summary>
    /// Save events.
    /// </summary>
    Task SaveAsync(CqrsAggregateRoot aggregate, IEnumerable<ICqrsEvent> events, CancellationToken cancellation = default);

    /// <summary>
    /// Copies an aggregate to offline storage and removes it from online logs.
    /// </summary>
    /// <remarks>
    /// Someone who is a purist with regard to event sourcing will red-flag this function and say the event stream 
    /// for an aggregate should never be altered or removed. However, we have two scenarios in which this is a non-
    /// negotiable business requirement. First, when a customer does not renew their contract with our business, we
    /// have a contractual obligation to remove all the customer's data from our systems. Second, we frequently run
    /// test-cases to confirm system functions are operating correctly; this data is temporary by definition, and 
    /// we do not want to permanently store the event streams for test aggregates.
    /// </remarks>
    void Box(Guid aggregateId);

    /// <summary>
    /// Copies an aggregate to offline storage and removes it from online logs.
    /// </summary>
    /// <remarks>
    /// Someone who is a purist with regard to event sourcing will red-flag this function and say the event stream 
    /// for an aggregate should never be altered or removed. However, we have two scenarios in which this is a non-
    /// negotiable business requirement. First, when a customer does not renew their contract with our business, we
    /// have a contractual obligation to remove all the customer's data from our systems. Second, we frequently run
    /// test-cases to confirm system functions are operating correctly; this data is temporary by definition, and 
    /// we do not want to permanently store the event streams for test aggregates.
    /// </remarks>
    Task BoxAsync(Guid aggregateId, CancellationToken cancellation = default);

    /// <summary>
    /// Retrieve event from file and insert back to event store
    /// </summary>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    IEnumerable<SerializedEvent> UnBox(Guid aggregateId);

    /// <summary>
    /// Retrieve event from file and insert back to event store
    /// </summary>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    Task<IEnumerable<SerializedEvent>> UnBoxAsync(Guid aggregateId, CancellationToken cancellation = default);
}
