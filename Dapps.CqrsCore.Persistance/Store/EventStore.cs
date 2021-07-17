using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
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
    public class EventStore : BaseStore<IEventDbContext>, IEventStore
    {
        private readonly string _offlineStorageFolder;
        public EventStore(ISerializer serializer, IServiceProvider service, EventStoreOptions options) : base(service)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(ISerializer));

            _offlineStorageFolder = options?.EventLocalStorage ??
                                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Commands");
        }

        public ISerializer Serializer { get; }

        public void Box(Guid aggregate)
        {
            var dbContext = GetDbContext();
            // Serialize the event stream and write it to an external file.
            var events = Get(aggregate, -1);
            foreach (var ev in events)
            {
                // Create a new directory using the aggregate identifier as the folder name.
                var path = Path.Combine(_offlineStorageFolder, aggregate.ToString());
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                //get json data from event
                var json = Serializer.Serialize(ev);
                var file = Path.Combine(path, $"{ev.Version}.json");
                File.WriteAllText(file, json, Encoding.Unicode);

                // Delete the aggregate and the events from the online logs.
                var existingEvent = dbContext.Events.SingleOrDefault(e =>
                    e.AggregateId.Equals(ev.AggregateId) && e.Version.Equals(ev.Version));
                if (existingEvent != null)
                {
                    dbContext.Events.Remove(existingEvent);
                }
            }

            //remove aggregate
            var existingAggregate = dbContext.Aggregates.AsNoTracking()
                .SingleOrDefault(e => e.AggregateId.Equals(aggregate));
            if (existingAggregate != null)
                dbContext.Aggregates.Remove(existingAggregate);

            dbContext.SaveChanges();
        }

        public bool Exists(Guid aggregateId)
        {
            return GetDbContext().Events.AsNoTracking().Any(x => x.AggregateId.Equals(aggregateId));
        }

        public bool Exists(Guid aggregateId, int version)
        {
            return GetDbContext().Events.AsNoTracking()
                .Any(x => x.AggregateId.Equals(aggregateId) && x.Version.Equals(version));
        }

        public IEnumerable<IEvent> Get(Guid aggregateId, int fromVersion)
        {
            return GetDbContext().Events.AsNoTracking()
                .Where(x => x.AggregateId.Equals(aggregateId) && x.Version >= fromVersion)
                .Select(x => x.Deserialize(Serializer)).ToList().AsEnumerable();
        }

        public IEnumerable<Guid> GetExpired(DateTimeOffset at)
        {
            return GetDbContext().Aggregates.AsNoTracking().Where(x => x.Expires != null && x.Expires <= at).Select(x => x.AggregateId)
                .ToList().AsEnumerable();
        }

        public void Save(AggregateRoot aggregate, IEnumerable<IEvent> events)
        {
            var dbContext = GetDbContext();

            var listEvents = new List<SerializedEvent>();

            foreach (var ev in events)
            {
                listEvents.Add(ev.Serialize(Serializer, aggregate.Id, ev.Version));
            }

            //using var transaction = Context.Database.BeginTransaction();

            EnsureAggregateExist(aggregate.Id, aggregate.GetType().Name.Replace("Aggregate", string.Empty),
                aggregate.GetType().FullName);

            foreach (var serializedEvent in listEvents)
            {
                dbContext.Events.Add(serializedEvent);
            }

            dbContext.SaveChanges();

            //transaction.Commit();
        }

        private void EnsureAggregateExist(Guid aggregateId, string className, string classType)
        {
            var dbContext = GetDbContext();
            if (!dbContext.Aggregates.AsNoTracking().Any(x => x.AggregateId.Equals(aggregateId)))
                dbContext.Aggregates.Add(new SerializedAggregate()
                {
                    AggregateId = aggregateId,
                    Class = className,
                    Type = classType

                });
        }
    }
}
