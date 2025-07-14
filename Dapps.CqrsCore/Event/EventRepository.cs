using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dapps.CqrsCore.Event;

/// <summary>
/// Repository for event to persist event to event store
/// </summary>
public class EventRepository : ICqrsEventRepository
{
    private readonly ICqrsEventStore _store;

    public EventRepository(ICqrsEventStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }
    
    /// <summary>
    /// Gets an id from the event store.
    /// </summary>
    public T Get<T>(Guid id) where T : CqrsAggregateRoot
    {
        return Rehydrate<T>(id);
    }

    public async Task<T> GetAsync<T>(Guid id) where T : CqrsAggregateRoot
    {
        return await RehydrateAsync<T>(id);
    }

    /// <summary>
    /// Saves all uncommitted changes to the event store.
    /// </summary>
    public IList<ICqrsEvent> Save<T>(T aggregate, int? version) where T : CqrsAggregateRoot
    {
        if (version != null && (_store.Exists(aggregate.Id, version.Value)))
            throw new ConcurrencyException(aggregate.Id);

        // Get the list of events that are not yet saved. 
        var events = aggregate.FlushUncommittedChanges();

        // Save the uncommitted changes.
        _store.Save(aggregate, events);

        // The event repository is not responsible for publishing these events. Instead they are returned to the 
        // caller for that purpose.
        return events;
    }

    public async Task<IList<ICqrsEvent>> SaveAsync<T>(T aggregate, int? version = null) where T : CqrsAggregateRoot
    {
        if (version != null && (await _store.ExistsAsync(aggregate.Id, version.Value)))
            throw new ConcurrencyException(aggregate.Id);

        // Get the list of events that are not yet saved. 
        var events = aggregate.FlushUncommittedChanges();

        // Save the uncommitted changes.
        await _store.SaveAsync(aggregate, events);

        // The event repository is not responsible for publishing these events. Instead they are returned to the 
        // caller for that purpose.
        return events;
    }

    /// <summary>
    /// Loads an id instance from the full history of events for that id.
    /// </summary>
    private T Rehydrate<T>(Guid id) where T : CqrsAggregateRoot
    {
        // Get all the events for the id.
        var events = _store.Get(id, -1);

        // Disallow empty event streams.
        if (!events.Any())
            throw new AggregateNotFoundException(typeof(T), id);

        // Create and load the id.
        var aggregate = AggregateFactory<T>.CreateAggregate();
        aggregate.ReHydrate(events);
        return aggregate;
    }
    
    private async Task<T> RehydrateAsync<T>(Guid id) where T : CqrsAggregateRoot
    {
        // Get all the events for the id.
        var events = await _store.GetAsync(id, -1);

        // Disallow empty event streams.
        if (!events.Any())
            throw new AggregateNotFoundException(typeof(T), id);

        // Create and load the id.
        var aggregate = AggregateFactory<T>.CreateAggregate();
        aggregate.ReHydrate(events);
        return aggregate;
    }

    #region Methods (boxing and unboxing)

    /// <summary>
    /// Copies an id to offline storage and removes it from online logs.
    /// </summary>
    /// <remarks>
    /// Aggregate boxing/unboxing is not implemented by default for all aggregates. It must be explicitly 
    /// implemented per id for those aggregates that require this functionality, and snapshots are required. 
    /// Therefore this function in this class throws a NotImplementedException; refer to SnapshotRepository for the
    /// implementation.
    /// </remarks>
    public void Box<T>(T aggregate) where T : CqrsAggregateRoot
    {
        throw new NotImplementedException();
    }

    public Task BoxAsync<T>(T aggregate) where T : CqrsAggregateRoot
    {
        throw new NotImplementedException();
    }

    public Task<T> UnboxAsync<T>(Guid aggregate) where T : CqrsAggregateRoot
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves an id from offline storage and returns only its most recent state.
    /// </summary>
    /// <remarks>
    /// Aggregate boxing/unboxing is not implemented by default for all aggregates. It must be explicitly 
    /// implemented per id for those aggregates that require this functionality, and snapshots are required. 
    /// Therefore this function in this class throws a NotImplementedException; refer to SnapshotRepository for the
    /// implementation.
    /// </remarks>
    public T Unbox<T>(Guid aggregateId) where T : CqrsAggregateRoot
    {
        throw new NotImplementedException();
    }

    #endregion
}
