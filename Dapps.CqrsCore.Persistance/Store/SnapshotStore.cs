using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapps.CqrsCore.Persistence.Exceptions;
using Dapps.CqrsCore.Snapshots;
using Microsoft.EntityFrameworkCore;

namespace Dapps.CqrsCore.Persistence.Store
{
    /// <summary>
    /// default snapshot store
    /// </summary>
    public class SnapshotStore : BaseStore<ISnapshotDbContext>, ISnapshotStore
    {
        private readonly string _offlineStorageFolder;
        private const string DefaultFolder = "Snapshot";

        public SnapshotStore(IServiceProvider service, SnapshotOptions configuration) : base(service)
        {
            _offlineStorageFolder =
                configuration?.LocalStorage ?? throw new ArgumentNullException(nameof(SnapshotOptions));
        }
        
        /// <summary>
        /// Boxing to save data to offline store and remove it from event store
        /// </summary>
        /// <param name="aggregate"></param>
        public void Box(Guid aggregate)
        {
            // Create a new directory using the aggregate identifier as the folder name.
            var path = Path.Combine(_offlineStorageFolder, DefaultFolder, aggregate.ToString());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // Serialize the event stream and write it to an external file.
            var snapshot = Get(aggregate);
            
            if (snapshot == null) return;

            var json = Get(aggregate).State;
            var file = Path.Combine(path, "Snapshot.json");
            File.WriteAllText(file, json, Encoding.Unicode);

            // Delete the aggregate and the events from the online logs.
            Delete(aggregate);
        }

        /// <summary>
        /// Get a snapshot based on aggregate id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Snapshot Get(Guid id)
        {
            var dbContext = GetDbContext();
            return dbContext.Snapshots.AsNoTracking().SingleOrDefault(e => e.AggregateId.Equals(id));
        }

        /// <summary>
        /// save data to snapshot store
        /// </summary>
        /// <param name="snapshot"></param>
        public void Save(Snapshot snapshot)
        {
            var dbContext = GetDbContext();
            var existingSnapshot = dbContext.Snapshots.AsNoTracking()
                .SingleOrDefault(e => e.AggregateId.Equals(snapshot.AggregateId));

            if (existingSnapshot == null)
            {
                dbContext.Snapshots.Add(snapshot);
            }
            else
            {
                existingSnapshot = new Snapshot()
                {
                    AggregateId = snapshot.AggregateId,
                    Version = snapshot.Version,
                    State = snapshot.State,
                    Time = snapshot.Time,
                };

                dbContext.Entry(existingSnapshot).State = EntityState.Modified;
            }

            dbContext.SaveChanges();
        }

        /// <summary>
        /// UnBoxing to load data from local storage and insert back to event store
        /// </summary>
        /// <param name="aggregate"></param>
        /// <returns></returns>
        public Snapshot Unbox(Guid aggregate)
        {
            // The snapshot must exist!
            var file = Path.Combine(_offlineStorageFolder, DefaultFolder, aggregate.ToString(), "Snapshot.json");
            if (!File.Exists(file))
                throw new SnapshotNotFoundException(file);

            // Read the serialized JSON into a new snapshot and return it.
            return new Snapshot
            {
                AggregateId = aggregate,
                Version = 1,
                State = File.ReadAllText(file, Encoding.Unicode)
            };
        }

        #region Methods (delete)
        /// <summary>
        /// Delete snapshot based on aggregate id
        /// </summary>
        /// <param name="aggregate"></param>
        
        private void Delete(Guid aggregate)
        {
            var dbContext = GetDbContext();

            var snapShot = Get(aggregate);

            if (snapShot == null) return;

            dbContext.Snapshots.Remove(snapShot);
            dbContext.SaveChanges();
        }

        #endregion

        private async Task DeleteAsync(Guid aggregate)
        {
            var dbContext = GetDbContext();

            var snapShot = await GetAsync(aggregate);

            if (snapShot == null) return;

            dbContext.Snapshots.Remove(snapShot);
            await dbContext.SaveChangesAsync();
        }

        public async Task<Snapshot> GetAsync(Guid id)
        {
            var dbContext = GetDbContext();
            return await dbContext.Snapshots.AsNoTracking().SingleOrDefaultAsync(e => e.AggregateId.Equals(id));
        }

        public async Task SaveAsync(Snapshot snapshot)
        {
            var dbContext = GetDbContext();
            var existingSnapshot = await dbContext.Snapshots.AsNoTracking()
                .SingleOrDefaultAsync(e => e.AggregateId.Equals(snapshot.AggregateId));

            if (existingSnapshot == null)
            {
                await dbContext.Snapshots.AddAsync(snapshot);
            }
            else
            {
                existingSnapshot = new Snapshot()
                {
                    AggregateId = snapshot.AggregateId,
                    Version = snapshot.Version,
                    State = snapshot.State,
                    Time = snapshot.Time,
                };

                dbContext.Entry(existingSnapshot).State = EntityState.Modified;
            }

            await dbContext.SaveChangesAsync();
        }
        
        public async Task BoxAsync(Guid aggregate)
        {
            // Create a new directory using the aggregate identifier as the folder name.
            var path = Path.Combine(_offlineStorageFolder, DefaultFolder, aggregate.ToString());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            // Serialize the event stream and write it to an external file.
            var snapshot = await GetAsync(aggregate);

            if (snapshot == null) return;

            var json = (await GetAsync(aggregate)).State;
            var file = Path.Combine(path, "Snapshot.json");
            await File.WriteAllTextAsync(file, json, Encoding.Unicode);

            // Delete the aggregate and the events from the online logs.
            await DeleteAsync(aggregate);
        }

        public async Task<Snapshot> UnboxAsync(Guid aggregate)
        {
            // The snapshot must exist!
            var file = Path.Combine(_offlineStorageFolder, DefaultFolder, aggregate.ToString(), "Snapshot.json");
            if (!File.Exists(file))
                throw new SnapshotNotFoundException(file);

            // Read the serialized JSON into a new snapshot and return it.
            return new Snapshot
            {
                AggregateId = aggregate,
                Version = 1,
                State = await File.ReadAllTextAsync(file, Encoding.Unicode)
            };
        }

    }
}
