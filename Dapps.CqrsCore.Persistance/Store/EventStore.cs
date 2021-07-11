using System;
using System.Collections.Generic;
using System.Linq;
using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Store
{
    public class EventStore : IEventStore
    {
        private readonly EventSourcingDBContext _dbContext;
        public EventStore(ISerializer serializer, EventSourcingDBContext dbContext)
        {
            Serializer = serializer;
            _dbContext = dbContext;
        }

        public ISerializer Serializer { get; }
        public void Box(Guid aggregate)
        {
            throw new NotImplementedException();
        }

        public bool Exists(Guid aggregateID)
        {
            return _dbContext.Events.AsNoTracking().Any(x => x.AggregateID.Equals(aggregateID));
        }

        public bool Exists(Guid aggregateID, int version)
        {
            return _dbContext.Events.AsNoTracking().Any(x => x.AggregateID.Equals(aggregateID) && x.Version.Equals(version));
        }

        public IEnumerable<IEvent> Get(Guid aggregateID, int fromVersion)
        {
            return _dbContext.Events.AsNoTracking()
                .Where(x => x.AggregateID.Equals(aggregateID) && x.Version >= fromVersion)
                .Select(x => x.Deserialize(Serializer)).ToList().AsEnumerable();
        }

        public IEnumerable<Guid> GetExpired(DateTimeOffset at)
        {
            return _dbContext.Aggregates.Where(x => x.Expires != null && x.Expires <= at).Select(x => x.AggregateID).ToList().AsEnumerable();
        }

        public void Save(AggregateRoot aggregate, IEnumerable<IEvent> events)
        {

            var listEvents = new List<SerializedEvent>();

            foreach (var ev in events)
            {
                listEvents.Add(ev.Serialize(Serializer, aggregate.Id, ev.Version, ev.UserId));
            }

            //using var transaction = _dbContext.Database.BeginTransaction();

            EnsureAggregateExist(aggregate.Id, aggregate.GetType().Name.Replace("Aggregate", string.Empty),
                aggregate.GetType().FullName, _dbContext);

            foreach (var serializedEvent in listEvents)
            {
                _dbContext.Events.Add(serializedEvent);
            }

            _dbContext.SaveChanges();

            //transaction.Commit();
        }

        private void EnsureAggregateExist(Guid aggregateID, string className, string classType, EventSourcingDBContext context)
        {
            if (!_dbContext.Aggregates.Any(x => x.AggregateID.Equals(aggregateID)))
                _dbContext.Aggregates.Add(new SerializedAggregate()
                {
                    AggregateID = aggregateID,
                    Class = className,
                    Type = classType

                });
        }
    }
}
