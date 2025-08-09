using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    public class CqrsEventStore : ICqrsEventStore
    {
        private readonly string _offlineStorageFolder;
        private const string DefaultFolder = "Events";
        private readonly ICqrsEventDbContext _dbContext;

        public CqrsEventStore(ICqrsSerializer serializer, IServiceProvider service, EventStoreOptions options, ICqrsEventDbContext dbContext)
        {
            Serializer = serializer ?? throw new ArgumentNullException(nameof(ICqrsSerializer));

            _offlineStorageFolder = options?.EventLocalStorage ??
                                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
            _dbContext = dbContext;
        }

        public ICqrsSerializer Serializer { get; }

        public void Box(Guid aggregateId)
        {
            var dbContext = _dbContext;
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
            return _dbContext.Events.AsNoTracking().Any(x => x.AggregateId.Equals(aggregateId));
        }

        public bool Exists(Guid aggregateId, int version)
        {
            return _dbContext.Events.AsNoTracking()
                .Any(x => x.AggregateId.Equals(aggregateId) && x.Version.Equals(version));
        }

        public IEnumerable<ICqrsEvent> Get(Guid aggregateId, int fromVersion)
        {
            return _dbContext.Events.AsNoTracking()
                .Where(x => x.AggregateId.Equals(aggregateId) && x.Version >= fromVersion)
                .Select(x => x.Deserialize(Serializer)).ToList().AsEnumerable();
        }

        public IEnumerable<Guid> GetExpired(DateTimeOffset at)
        {
            return _dbContext.Aggregates.AsNoTracking().Where(x => x.Expires != null && x.Expires <= at).Select(x => x.AggregateId)
                .ToList().AsEnumerable();
        }

        public void Save(CqrsAggregateRoot aggregate, IEnumerable<ICqrsEvent> events)
        {
            var dbContext = _dbContext;

            var listEvents = new List<SerializedEvent>();

            foreach (var ev in events)
            {
                listEvents.Add(ev.Serialize(Serializer, aggregate.Id, ev.Version));
            }

            dbContext.BeginTransaction();

            EnsureAggregateExist(aggregate.Id, aggregate.GetType().Name.Replace("Aggregate", string.Empty),
                aggregate.GetType().FullName);

            foreach (var serializedEvent in listEvents)
            {
                dbContext.Events.Add(serializedEvent);
            }

            dbContext.SaveChanges();

            dbContext.Commit();
        }

        private void EnsureAggregateExist(Guid aggregateId, string className, string classType)
        {
            var dbContext = _dbContext;
            if (!dbContext.Aggregates.AsNoTracking().Any(x => x.AggregateId.Equals(aggregateId)))
                dbContext.Aggregates.Add(new SerializedAggregate()
                {
                    AggregateId = aggregateId,
                    Class = className,
                    Type = classType

                });
        }

        public async Task<bool> ExistsAsync(Guid aggregateId, CancellationToken cancellation = default)
        {
            return await _dbContext.Events.AsNoTracking().AnyAsync(x => x.AggregateId.Equals(aggregateId), cancellation);
        }

        public async Task<bool> ExistsAsync(Guid aggregateId, int version, CancellationToken cancellation = default)
        {
            return await _dbContext.Events.AsNoTracking()
                .AnyAsync(x => x.AggregateId.Equals(aggregateId) && x.Version.Equals(version), cancellation);
        }

        public async Task<IEnumerable<ICqrsEvent>> GetAsync(Guid aggregateId, int fromVersion, CancellationToken cancellation = default)
        {
            return await _dbContext.Events.AsNoTracking()
                .Where(x => x.AggregateId.Equals(aggregateId) && x.Version >= fromVersion)
                .Select(x => x.Deserialize(Serializer)).ToListAsync(cancellation);
        }

        public async Task<IEnumerable<Guid>> GetExpiredAsync(DateTimeOffset at, CancellationToken cancellation = default)
        {
            return await _dbContext.Aggregates.AsNoTracking().Where(x => x.Expires != null && x.Expires <= at).Select(x => x.AggregateId)
                .ToListAsync(cancellation);
        }

        public async Task SaveAsync(CqrsAggregateRoot aggregate, IEnumerable<ICqrsEvent> events, CancellationToken cancellation = default)
        {
            var dbContext = _dbContext;

            var listEvents = new List<SerializedEvent>();

            foreach (var ev in events)
            {
                listEvents.Add(ev.Serialize(Serializer, aggregate.Id, ev.Version));
            }

            await dbContext.BeginTransactionAsync(cancellation);

            try
            {
                EnsureAggregateExist(aggregate.Id, aggregate.GetType().Name.Replace("Aggregate", string.Empty),
                        aggregate.GetType().FullName);

                foreach (var serializedEvent in listEvents)
                {
                    await dbContext.Events.AddAsync(serializedEvent, cancellation);
                }

                await dbContext.SaveChangesAsync(cancellation);

                await dbContext.CommitAsync(cancellation);
            }
            catch (Exception)
            {
                await dbContext.RollbackAsync(cancellation);
                throw;
            }
        }

        public async Task BoxAsync(Guid aggregateId, CancellationToken cancellation = default)
        {

            var dbContext = _dbContext;

            try
            {
                await dbContext.BeginTransactionAsync(cancellation);
                // Serialize the event stream and write it to an external file.
                var events = await GetAsync(aggregateId, -1, cancellation);
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
                    await File.WriteAllTextAsync(file, data, Encoding.Unicode, cancellation);

                    // Delete the aggregate and the events from the online logs.
                    var existingEvent = await dbContext.Events.SingleOrDefaultAsync(e =>
                        e.AggregateId.Equals(ev.AggregateId) && e.Version.Equals(ev.Version), cancellation);
                    if (existingEvent != null)
                    {
                        dbContext.Events.Remove(existingEvent);
                    }
                }

                //remove aggregate
                var existingAggregate = await dbContext.Aggregates
                    .SingleOrDefaultAsync(e => e.AggregateId.Equals(aggregateId), cancellation);

                if (existingAggregate != null)
                {
                    dbContext.Aggregates.Remove(existingAggregate);
                }

                await dbContext.SaveChangesAsync(cancellation);

                await dbContext.CommitAsync(cancellation);
            }
            catch (Exception)
            {
                await dbContext.RollbackAsync(cancellation);
                throw;
            }
        }

        public async Task<IEnumerable<SerializedEvent>> UnBoxAsync(Guid aggregate, CancellationToken cancellation = default)
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

                var eventJson = await File.ReadAllTextAsync(file, Encoding.Unicode, cancellation);

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
