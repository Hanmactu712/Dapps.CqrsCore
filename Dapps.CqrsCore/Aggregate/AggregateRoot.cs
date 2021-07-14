using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapps.CqrsCore.Aggregate
{
    public abstract class AggregateRoot
    {
        private readonly IList<IEvent> _changes = new List<IEvent>();

        public AggregateState State { get; set; }

        public Guid Id { get; set; }

        public int Version { get; set; }

        public abstract AggregateState CreateState();

        public IList<IEvent> GetUncommittedChanges()
        {
            lock (_changes)
            {
                return _changes.ToList();
            }
        }

        public IList<IEvent> FlushUncommittedChanges()
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

        public void ReHydrate(IEnumerable<IEvent> histories)
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

        protected void Apply(IEvent change)
        {
            lock (_changes)
            {
                ApplyEvent(change);
                _changes.Add(change);
            }
        }

        protected virtual void ApplyEvent(IEvent change)
        {
            if (State == null)
                State = CreateState();

            State.Apply(change);
        }
        //private IList<>
    }
}
