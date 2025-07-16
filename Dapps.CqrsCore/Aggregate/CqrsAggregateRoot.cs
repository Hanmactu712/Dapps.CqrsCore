using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapps.CqrsCore.Aggregate;

public abstract class CqrsAggregateRoot
{
    private readonly IList<ICqrsEvent> _changes = new List<ICqrsEvent>();

    /// <summary>
    /// State of the aggregate, it is used to store the current state of the aggregate
    /// </summary>
    public AggregateState State { get; set; }

    /// <summary>
    /// Unique Id of the aggregate, it is used to identify the aggregate in the event store
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Version of the aggregate, it is incremented on each event applied
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Initialize a new instance of the CqrsAggregateRoot class
    /// </summary>
    /// <returns></returns>
    public abstract AggregateState CreateState();

    /// <summary>
    /// Get the uncommitted changes of the aggregate, it is used to get the events that are not yet persisted
    /// </summary>
    /// <returns></returns>
    public IList<ICqrsEvent> GetUncommittedChanges()
    {
        lock (_changes)
        {
            return _changes.ToList();
        }
    }

    /// <summary>
    /// Flush the uncommitted changes of the aggregate, it is used to persist the events that are not yet persisted
    /// </summary>
    /// <returns></returns>
    /// <exception cref="MissingAggregateIdentifierException"></exception>
    public IList<ICqrsEvent> FlushUncommittedChanges()
    {
        lock (_changes)
        {
            var changes = _changes.ToArray();
            int i = 0;
            foreach (var change in changes)
            {
                if (change.AggregateId == Guid.Empty && Id == Guid.Empty)
                {
                    throw new MissingAggregateIdentifierException(GetType(), change.GetType());
                }

                if (change.AggregateId == Guid.Empty)
                    change.AggregateId = Id;
                i++;
                change.Version = Version + i;
                change.Time = DateTimeOffset.UtcNow;
            }

            Version += changes.Length;

            _changes.Clear();
            return changes;
        }
    }

    /// <summary>
    /// Re-hydrate the aggregate state from a list of events, it is used to restore the state of the aggregate from the event store
    /// </summary>
    /// <param name="histories"></param>
    /// <exception cref="UnorderedEventsException"></exception>
    public void ReHydrate(IEnumerable<ICqrsEvent> histories)
    {
        lock (_changes)
        {
            foreach (var change in histories.ToArray())
            {
                if (change.Version != Version + 1)
                {
                    throw new UnorderedEventsException(change.AggregateId);
                }

                ApplyEvent(change);

                Id = change.AggregateId;
                Version++;
            }
        }
    }

    /// <summary>
    /// Apply an unboxing event to re-hydrate the aggregate state.
    /// </summary>
    /// <param name="ev"></param>
    public abstract void ApplyUnBoxingEvent(ICqrsEvent ev);

    /// <summary>
    /// Apply an event to the aggregate, it is used to apply a new event to the aggregate and store it in the uncommitted changes list.
    /// </summary>
    /// <param name="change"></param>
    protected void Apply(ICqrsEvent change)
    {
        lock (_changes)
        {
            ApplyEvent(change);
            _changes.Add(change);
        }
    }

    /// <summary>
    /// Apply an event to the aggregate state, it is used to apply a new event to the aggregate state.
    /// </summary>
    /// <param name="change"></param>
    protected virtual void ApplyEvent(ICqrsEvent change)
    {
        if (State == null)
            State = CreateState();

        State.Apply(change);
    }
}
