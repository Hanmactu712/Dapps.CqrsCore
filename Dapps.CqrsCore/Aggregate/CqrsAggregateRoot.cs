using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapps.CqrsCore.Aggregate;

public abstract class CqrsAggregateRoot
{
    private readonly IList<ICqrsEvent> _changes = new List<ICqrsEvent>();

    public AggregateState State { get; set; }

    public Guid Id { get; set; }

    public int Version { get; set; }

    public abstract AggregateState CreateState();

    public IList<ICqrsEvent> GetUncommittedChanges()
    {
        lock (_changes)
        {
            return _changes.ToList();
        }
    }

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

    public abstract void ApplyUnBoxingEvent(ICqrsEvent ev);

    protected void Apply(ICqrsEvent change)
    {
        lock (_changes)
        {
            ApplyEvent(change);
            _changes.Add(change);
        }
    }

    protected virtual void ApplyEvent(ICqrsEvent change)
    {
        if (State == null)
            State = CreateState();

        State.Apply(change);
    }
}
