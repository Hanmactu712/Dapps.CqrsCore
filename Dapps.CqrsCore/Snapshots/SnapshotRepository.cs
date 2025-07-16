using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapps.CqrsCore.Exceptions;
using System.Threading.Tasks;


namespace Dapps.CqrsCore.Snapshots;

/// <summary>
/// Saves and gets snapshots to and from a snapshot store.
/// </summary>
public class SnapshotRepository : ICqrsEventRepository
{
    private readonly GuidCache<CqrsAggregateRoot> _cache = new GuidCache<CqrsAggregateRoot>();

    private readonly ISnapshotStore _snapshotStore;
    private readonly ISnapshotStrategy _snapshotStrategy;
    private readonly ICqrsEventRepository _eventRepository;
    private readonly ICqrsEventStore _eventStore;

    /// <summary>
    /// Constructs a new SnapshotRepository instance.
    /// </summary>
    /// <param name="eventStore">Store where events are persisted</param>
    /// <param name="eventRepository">Repository to get aggregates from the event store</param>
    /// <param name="snapshotStore">Store where snapshots are persisted</param>
    /// <param name="snapshotStrategy">Strategy used to determine when to take a snapshot</param>
    public SnapshotRepository(ICqrsEventStore eventStore, ICqrsEventRepository eventRepository, ISnapshotStore snapshotStore, ISnapshotStrategy snapshotStrategy)
    {
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _eventRepository = eventRepository ?? throw new ArgumentNullException(nameof(eventRepository));
        _snapshotStore = snapshotStore ?? throw new ArgumentNullException(nameof(snapshotStore));
        _snapshotStrategy = snapshotStrategy ?? throw new ArgumentNullException(nameof(snapshotStrategy));
    }

    /// <summary>
    /// Saves the aggregate. Takes a snapshot if needed.
    /// </summary>
    public IList<ICqrsEvent> Save<T>(T aggregate, int? version = null) where T : CqrsAggregateRoot
    {
        // Cache the aggregate for 5 minutes.
        _cache.Add(aggregate.Id, aggregate, 5 * 60, true);

        // Take a snapshot if needed.
        TakeSnapshot(aggregate, false);

        // Return the stream of saved events to the caller so they can be published.
        return _eventRepository.Save(aggregate, version);
    }

    /// <summary>
    /// Gets the aggregate.
    /// </summary>
    public T Get<T>(Guid aggregateId) where T : CqrsAggregateRoot
    {
        // Check the cache to see if the aggregate is already in memory.
        var snapshot = _cache.Get(aggregateId);
        if (snapshot != null)
            return (T)snapshot;

        // If it is not in the cache then load the aggregate from the most recent snapshot.
        var aggregate = AggregateFactory<T>.CreateAggregate();
        var snapshotVersion = RestoreAggregateFromSnapshot(aggregateId, aggregate);

        // If there is no snapshot then load the aggregate directly from the event store.
        if (snapshotVersion == -1)
            return _eventRepository.Get<T>(aggregateId);

        // Otherwise load the aggregate from the events that occurred after the snapshot was taken.
        var events = (_eventStore.Get(aggregateId, snapshotVersion))
            .Where(desc => desc.Version > snapshotVersion);

        aggregate.ReHydrate(events);

        return aggregate;
    }

    /// <summary>
    /// Loads the aggregate from the most recent snapshot.
    /// </summary>
    /// <returns>
    /// Returns the version number for the aggregate when the snapshot was taken.
    /// </returns>
    private int RestoreAggregateFromSnapshot<T>(Guid id, T aggregate) where T : CqrsAggregateRoot
    {
        var snapshot = _snapshotStore.Get(id);

        if (snapshot == null)
            return -1;

        aggregate.Id = snapshot.AggregateId;
        aggregate.Version = snapshot.Version;
        aggregate.State = _eventStore.Serializer.Deserialize<AggregateState>(snapshot.State, aggregate.CreateState().GetType());

        return snapshot.Version;
    }
    
    private async Task<int> RestoreAggregateFromSnapshotAsync<T>(Guid id, T aggregate) where T : CqrsAggregateRoot
    {
        var snapshot = await _snapshotStore.GetAsync(id);

        if (snapshot == null)
            return -1;

        aggregate.Id = snapshot.AggregateId;
        aggregate.Version = snapshot.Version;
        aggregate.State = _eventStore.Serializer.Deserialize<AggregateState>(snapshot.State, aggregate.CreateState().GetType());

        return snapshot.Version;
    }

    /// <summary>
    /// Saves a snapshot of the aggregate if the strategy indicates a snapshot should now be taken.
    /// </summary>
    private void TakeSnapshot(CqrsAggregateRoot aggregate, bool force)
    {
        if (!force && !_snapshotStrategy.ShouldTakeSnapShot(aggregate))
            return;

        var snapshot = new Snapshot
        {
            AggregateId = aggregate.Id,
            Version = aggregate.Version,
            State = _eventStore.Serializer.Serialize(aggregate.State),
            Time = DateTimeOffset.Now
        };

        snapshot.Version = aggregate.Version + aggregate.GetUncommittedChanges().Count;

        _snapshotStore.Save(snapshot);
    }

    private async Task TakeSnapshotAsync(CqrsAggregateRoot aggregate, bool force)
    {
        if (!force && !_snapshotStrategy.ShouldTakeSnapShot(aggregate))
            return;

        var snapshot = new Snapshot
        {
            AggregateId = aggregate.Id,
            Version = aggregate.Version,
            State = _eventStore.Serializer.Serialize(aggregate.State),
            Time = DateTimeOffset.Now
        };

        snapshot.Version = aggregate.Version + aggregate.GetUncommittedChanges().Count;

        await _snapshotStore.SaveAsync(snapshot);
    }

    #region Methods (boxing and unboxing)

    /// <summary>
    /// Checks for expired aggregates. Automatically boxes all aggregates for which the timer is now elapsed.
    /// </summary>
    public void Ping()
    {
        var aggregates = _eventStore.GetExpired(DateTimeOffset.UtcNow);
        foreach (var aggregate in aggregates)
            Box(Get<CqrsAggregateRoot>(aggregate));
    }

    /// <summary>
    /// Copies an aggregate to offline storage and removes it from online logs.
    /// </summary>
    public void Box<T>(T aggregate) where T : CqrsAggregateRoot
    {
        TakeSnapshot(aggregate, true);

        _snapshotStore.Box(aggregate.Id);
        _eventStore.Box(aggregate.Id);

        _cache.Remove(aggregate.Id);
    }

    /// <summary>
    /// Retrieves an aggregate from offline storage and returns only its most recent state.
    /// </summary>
    public T Unbox<T>(Guid aggregateId) where T : CqrsAggregateRoot
    {
        //var snapshot = _snapshotStore.Unbox(aggregateId);
        var aggregate = AggregateFactory<T>.CreateAggregate();
        aggregate.Id = aggregateId;
        //aggregate.Version = 1;

        var events = _eventStore.UnBox(aggregateId).ToList();

        if (!events.Any()) return aggregate;

        foreach (var serializedEvent in events.OrderBy(e => e.Version))
        {
            var concreteEvent = GetEventFromSerializedEvent(serializedEvent);
            if (concreteEvent != null)
            {
                InvokeApplyMethod(aggregate, concreteEvent);
            }
        }
        //var createCommand = _eventStore.Serializer.Deserialize<AggregateState>(snapshot.State, aggregate.CreateState().GetType());
        //aggregate.State = _eventStore.Serializer.Deserialize<AggregateState>(snapshot.State, aggregate.CreateState().GetType());
        return aggregate;

    }

    /// <summary>
    /// Invokes the ApplyUnBoxingEvent method on the aggregate to apply the event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="aggregate"></param>
    /// <param name="ev"></param>
    /// <exception cref="MethodNotFoundException"></exception>
    private void InvokeApplyMethod<T>(T aggregate, ICqrsEvent ev) where T : CqrsAggregateRoot
    {
        var aggregateType = aggregate.GetType();
        var apply = aggregateType.GetMethod("ApplyUnBoxingEvent", new[] { typeof(ICqrsEvent) });

        if (apply == null)
        {
            throw new MethodNotFoundException(aggregateType, "ApplyUnBoxingEvent", typeof(ICqrsEvent));
        }

        apply.Invoke(aggregate, new object[] { ev });
    }

    /// <summary>
    /// Retrieves the concrete event type from the serialized event data.
    /// </summary>
    /// <param name="serializedEvent"></param>
    /// <returns></returns>
    private ICqrsEvent GetEventFromSerializedEvent(SerializedEvent serializedEvent)
    {
        var eventType = serializedEvent.Type;

        if (string.IsNullOrEmpty(eventType))
            return null;

        var entryAssembly = Assembly.GetExecutingAssembly();

        var type = entryAssembly.GetType(eventType);

        if (type == null)
        {
            entryAssembly = Assembly.GetCallingAssembly();

            type = entryAssembly.GetType(eventType);

            if (type == null)
            {
                entryAssembly = Assembly.GetEntryAssembly();

                if (entryAssembly != null)
                    type = entryAssembly.GetType(eventType);

            }

        }

        if (type == null)
        {
            return null;
        }

        var ev = _eventStore.Serializer.Deserialize<ICqrsEvent>(serializedEvent.Data, type);

        return ev;
    }

    /// <summary>
    /// Retrieves an aggregate from the snapshot store or event store asynchronously.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    public async Task<T> GetAsync<T>(Guid aggregateId) where T : CqrsAggregateRoot
    {
        // Check the cache to see if the aggregate is already in memory.
        var snapshot = _cache.Get(aggregateId);
        if (snapshot != null)
            return (T)snapshot;

        // If it is not in the cache then load the aggregate from the most recent snapshot.
        var aggregate = AggregateFactory<T>.CreateAggregate();
        var snapshotVersion = await RestoreAggregateFromSnapshotAsync(aggregateId, aggregate);

        // If there is no snapshot then load the aggregate directly from the event store.
        if (snapshotVersion == -1)
            return await _eventRepository.GetAsync<T>(aggregateId);

        // Otherwise load the aggregate from the events that occurred after the snapshot was taken.
        var events = (await _eventStore.GetAsync(aggregateId, snapshotVersion))
            .Where(desc => desc.Version > snapshotVersion);

        aggregate.ReHydrate(events);

        return aggregate;
    }

    /// <summary>
    /// Saves the aggregate asynchronously. Takes a snapshot if needed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="aggregate"></param>
    /// <param name="version"></param>
    /// <returns></returns>
    public async Task<IList<ICqrsEvent>> SaveAsync<T>(T aggregate, int? version = null) where T : CqrsAggregateRoot
    {
        // Cache the aggregate for 5 minutes.
        _cache.Add(aggregate.Id, aggregate, 5 * 60, true);

        // Take a snapshot if needed.
        await TakeSnapshotAsync(aggregate, false);

        // Return the stream of saved events to the caller so they can be published.
        return await _eventRepository.SaveAsync(aggregate, version);
    }

    /// <summary>
    /// Copies an aggregate to offline storage and removes it from online logs asynchronously.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="aggregate"></param>
    /// <returns></returns>
    public async Task BoxAsync<T>(T aggregate) where T : CqrsAggregateRoot
    {
        await TakeSnapshotAsync(aggregate, true);

        _snapshotStore.Box(aggregate.Id);
        await _eventStore.BoxAsync(aggregate.Id);

        _cache.Remove(aggregate.Id);
    }

    /// <summary>
    /// Retrieves an aggregate from offline storage and returns only its most recent state asynchronously.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    public async Task<T> UnboxAsync<T>(Guid aggregateId) where T : CqrsAggregateRoot
    {
        //var snapshot = _snapshotStore.Unbox(aggregateId);
        var aggregate = AggregateFactory<T>.CreateAggregate();
        aggregate.Id = aggregateId;
        //aggregate.Version = 1;

        var unBoxEvents = await _eventStore.UnBoxAsync(aggregateId);
        var events = unBoxEvents.ToList();

        if (!events.Any()) return aggregate;

        foreach (var serializedEvent in events.OrderBy(e => e.Version))
        {
            var concreteEvent = GetEventFromSerializedEvent(serializedEvent);
            if (concreteEvent != null)
            {
                InvokeApplyMethod(aggregate, concreteEvent);
            }
        }

        return aggregate;
    }

    #endregion
}
