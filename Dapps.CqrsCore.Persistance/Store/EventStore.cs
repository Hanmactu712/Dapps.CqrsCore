using System;
using System.Collections.Generic;
using System.Linq;
using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dapps.CqrsCore.Persistence.Store
{
    /// <summary>
    /// default event store
    /// </summary>
    public class EventStore : IEventStore
    {
        private readonly IEventDbContext _dbContext;

        public EventStore(ISerializer serializer, IServiceProvider service)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(ISerializer));
            _dbContext = service.CreateScope().ServiceProvider.GetRequiredService<IEventDbContext>();

            if (_dbContext == null)
                throw new ArgumentNullException(nameof(IEventDbContext));
        }

        public ISerializer Serializer { get; }

        public void Box(Guid aggregate)
        {
            throw new NotImplementedException();
        }

        public bool Exists(Guid aggregateId)
        {
            return _dbContext.Events.AsNoTracking().Any(x => x.AggregateId.Equals(aggregateId));
        }

        public bool Exists(Guid aggregateId, int version)
        {
            return _dbContext.Events.AsNoTracking()
                .Any(x => x.AggregateId.Equals(aggregateId) && x.Version.Equals(version));
        }

        public IEnumerable<IEvent> Get(Guid aggregateId, int fromVersion)
        {
            return _dbContext.Events.AsNoTracking()
                .Where(x => x.AggregateId.Equals(aggregateId) && x.Version >= fromVersion)
                .Select(x => x.Deserialize(Serializer)).ToList().AsEnumerable();
        }

        public IEnumerable<Guid> GetExpired(DateTimeOffset at)
        {
            return _dbContext.Aggregates.Where(x => x.Expires != null && x.Expires <= at).Select(x => x.AggregateId)
                .ToList().AsEnumerable();
        }

        public void Save(AggregateRoot aggregate, IEnumerable<IEvent> events)
        {

            var listEvents = new List<SerializedEvent>();

            foreach (var ev in events)
            {
                listEvents.Add(ev.Serialize(Serializer, aggregate.Id, ev.Version));
            }

            //using var transaction = _dbContext.Database.BeginTransaction();

            EnsureAggregateExist(aggregate.Id, aggregate.GetType().Name.Replace("Aggregate", string.Empty),
                aggregate.GetType().FullName);

            foreach (var serializedEvent in listEvents)
            {
                _dbContext.Events.Add(serializedEvent);
            }

            _dbContext.SaveChanges();

            //transaction.Commit();
        }

        private void EnsureAggregateExist(Guid aggregateId, string className, string classType)
        {
            if (!_dbContext.Aggregates.Any(x => x.AggregateId.Equals(aggregateId)))
                _dbContext.Aggregates.Add(new SerializedAggregate()
                {
                    AggregateId = aggregateId,
                    Class = className,
                    Type = classType

                });
        }
    }
}
