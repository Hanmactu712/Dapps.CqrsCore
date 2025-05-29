using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapps.CqrsCore.Aggregate;
using Dapps.CqrsCore.Event;
using Dapps.CqrsCore.Persistence.Exceptions;
using Dapps.CqrsCore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Store
{
    /// <summary>
    /// default event store
    /// </summary>
    public class EventStore : BaseStore<IEventDbContext>, IEventStore
    {
        private readonly string _offlineStorageFolder;
        private const string DefaultFolder = "Events";
        public EventStore(ISerializer serializer, IServiceProvider service, EventStoreOptions options) : base(service)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(ISerializer));

            _offlineStorageFolder = options?.EventLocalStorage ??
                                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
        }

        public ISerializer Serializer { get; }

        public void Box(Guid aggregateId)
        {
            var dbContext = GetDbContext();
            // Serialize the event stream and write it to an external file.
            var events = Get(aggregateId, -1);
            foreach (var ev in events)
            {
                // Create a new directory using the aggregate identifier as the folder name.
                var path = Path.Combine(_offlineStorageFolder, DefaultFolder, aggregateId.ToString());
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                //get json data from event
                var serializedEvent = ev.Serialize(Serializer, aggregateId, ev.Version);
                var data = Serializer.Serialize(serializedEvent);
                var file = Path.Combine(path, $"{ev.Version}.json");
                File.WriteAllText(file, data, Encoding.Unicode);

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
                .SingleOrDefault(e => e.AggregateId.Equals(aggregateId));

            if (existingAggregate != null)
                dbContext.Aggregates.Remove(existingAggregate);

            dbContext.SaveChanges();
        }

        public IEnumerable<SerializedEvent> UnBox(Guid aggregateId)
        {
            var filePath = Path.Combine(_offlineStorageFolder, DefaultFolder, aggregateId.ToString());

            if (!Directory.Exists(filePath))
                throw new EventNotFoundException(filePath);

            var files = Directory.GetFiles(filePath).OrderBy(s => s).ToList();
            if (files.Count <= 0)
                throw new EventNotFoundException(filePath);

            var result = new List<SerializedEvent>();

            foreach (var file in files)
            {
                if (!File.Exists(file)) continue;

                var eventJson = File.ReadAllText(file, Encoding.Unicode);

                var serializedEvent = Serializer.Deserialize<SerializedEvent>(eventJson, typeof(SerializedEvent));

                if (serializedEvent != null && !serializedEvent.AggregateId.Equals(Guid.Empty))
                {
                    result.Add(serializedEvent);
                }
            }

            return result;
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

        public async Task<bool> ExistsAsync(Guid aggregateId)
        {
            return await GetDbContext().Events.AsNoTracking().AnyAsync(x => x.AggregateId.Equals(aggregateId));
        }

        public async Task<bool> ExistsAsync(Guid aggregateId, int version)
        {
            return await GetDbContext().Events.AsNoTracking()
                .AnyAsync(x => x.AggregateId.Equals(aggregateId) && x.Version.Equals(version));
        }

        public async Task<IEnumerable<IEvent>> GetAsync(Guid aggregateId, int fromVersion)
        {
            return await GetDbContext().Events.AsNoTracking()
                .Where(x => x.AggregateId.Equals(aggregateId) && x.Version >= fromVersion)
                .Select(x => x.Deserialize(Serializer)).ToListAsync();
        }

        public async Task<IEnumerable<Guid>> GetExpiredAsync(DateTimeOffset at)
        {
            return await GetDbContext().Aggregates.AsNoTracking().Where(x => x.Expires != null && x.Expires <= at).Select(x => x.AggregateId)
                .ToListAsync();
        }

        public async Task SaveAsync(AggregateRoot aggregate, IEnumerable<IEvent> events)
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
                await dbContext.Events.AddAsync(serializedEvent);
            }

            dbContext.SaveChanges();

            //transaction.Commit();
        }

        public async Task BoxAsync(Guid aggregateId)
        {
            var dbContext = GetDbContext();
            // Serialize the event stream and write it to an external file.
            var events = await GetAsync(aggregateId, -1);
            foreach (var ev in events)
            {
                // Create a new directory using the aggregate identifier as the folder name.
                var path = Path.Combine(_offlineStorageFolder, DefaultFolder, aggregateId.ToString());
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                //get json data from event
                var serializedEvent = ev.Serialize(Serializer, aggregateId, ev.Version);
                var data = Serializer.Serialize(serializedEvent);
                var file = Path.Combine(path, $"{ev.Version}.json");
                File.WriteAllText(file, data, Encoding.Unicode);

                // Delete the aggregate and the events from the online logs.
                var existingEvent = dbContext.Events.SingleOrDefault(e =>
                    e.AggregateId.Equals(ev.AggregateId) && e.Version.Equals(ev.Version));
                if (existingEvent != null)
                {
                    dbContext.Events.Remove(existingEvent);
                }
            }

            //remove aggregate
            var existingAggregate = await dbContext.Aggregates.AsNoTracking()
                .SingleOrDefaultAsync(e => e.AggregateId.Equals(aggregateId));

            if (existingAggregate != null)
                dbContext.Aggregates.Remove(existingAggregate);

            dbContext.SaveChanges();
        }

        public async Task<IEnumerable<SerializedEvent>> UnBoxAsync(Guid aggregate)
        {
            var filePath = Path.Combine(_offlineStorageFolder, DefaultFolder, aggregate.ToString());

            if (!Directory.Exists(filePath))
                throw new EventNotFoundException(filePath);

            var files = Directory.GetFiles(filePath).OrderBy(s => s).ToList();
            if (files.Count <= 0)
                throw new EventNotFoundException(filePath);

            var result = new List<SerializedEvent>();

            foreach (var file in files)
            {
                if (!File.Exists(file)) continue;

                var eventJson = await File.ReadAllTextAsync(file, Encoding.Unicode);

                var serializedEvent = Serializer.Deserialize<SerializedEvent>(eventJson, typeof(SerializedEvent));

                if (serializedEvent != null && !serializedEvent.AggregateId.Equals(Guid.Empty))
                {
                    result.Add(serializedEvent);
                }
            }

            return result;
        }
    }
}
